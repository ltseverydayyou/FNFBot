using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using WindowsInput.Native;
using FridayNightFunkin;

namespace FNFBot20
{
    public class Bot : IDisposable
    {
        public static bool Playing = false;
        public static bool SongLoaded = false;
        public static Stopwatch watch { get; set; }

        public string sngDir { get; set; }
        public KeyBot kBot;
        public MapBot mBot;
        public RenderBot rBot;

        public Thread currentPlayThread { get; set; }

        private readonly object threadLock = new object();
        private readonly List<Thread> workerThreads = new List<Thread>();
        private readonly object heldKeyLock = new object();
        private readonly Dictionary<VirtualKeyCode, int> heldKeys = new Dictionary<VirtualKeyCode, int>();
        private volatile bool shutdownRequested;
        private int playbackGeneration;
        private readonly List<ScheduledInputEvent> inputSchedule = new List<ScheduledInputEvent>();
        private bool highDensityChart;
        private int peakNotesPerSecond;
        private double minimumLaneGapMs = double.PositiveInfinity;
        private int maximumSectionHitNotes;

        [DllImport("winmm.dll")]
        private static extern uint timeBeginPeriod(uint uPeriod);

        [DllImport("winmm.dll")]
        private static extern uint timeEndPeriod(uint uPeriod);

        private sealed class ScheduledNote
        {
            public double Time;
            public double Length;
            public int Lane;
        }

        private sealed class ScheduledInputEvent
        {
            public double Time;
            public int Lane;
            public bool IsDown;
        }

        private static readonly Dictionary<int, int[]> DefaultLaneLayouts = new Dictionary<int, int[]>
        {
            { 1, new[] { 32 } },
            { 2, new[] { 68, 75 } },
            { 3, new[] { 68, 32, 75 } },
            { 4, new[] { 0x25, 0x28, 0x26, 0x27 } },
            { 5, new[] { 68, 70, 32, 74, 75 } },
            { 6, new[] { 83, 68, 70, 74, 75, 76 } },
            { 7, new[] { 83, 68, 70, 32, 74, 75, 76 } },
            { 8, new[] { 65, 83, 68, 70, 72, 74, 75, 76 } },
            { 9, new[] { 65, 83, 68, 70, 32, 72, 74, 75, 76 } },
            { 10, new[] { 65, 83, 68, 70, 71, 32, 72, 74, 75, 76 } },
            { 11, new[] { 65, 83, 68, 70, 71, 32, 72, 74, 75, 76, 190 } },
            { 12, new[] { 65, 83, 68, 70, 67, 86, 78, 77, 72, 74, 75, 76 } },
            { 13, new[] { 65, 83, 68, 70, 67, 86, 32, 78, 77, 72, 74, 75, 76 } },
            { 14, new[] { 65, 83, 68, 70, 67, 86, 84, 89, 78, 77, 72, 74, 75, 76 } },
            { 15, new[] { 65, 83, 68, 70, 67, 86, 84, 89, 85, 78, 77, 72, 74, 75, 76 } },
            { 16, new[] { 65, 83, 68, 70, 81, 87, 69, 82, 89, 85, 73, 79, 72, 74, 75, 76 } },
            { 17, new[] { 65, 83, 68, 70, 81, 87, 69, 82, 32, 89, 85, 73, 79, 72, 74, 75, 76 } },
            { 18, new[] { 65, 83, 68, 70, 32, 72, 74, 75, 76, 81, 87, 69, 82, 84, 89, 85, 73, 79 } }
        };

        private readonly Dictionary<int, int[]> laneLayouts = new Dictionary<int, int[]>();
        private static readonly string LaneKeybindFile = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "lane-keybinds.settings");

        public Bot()
        {
            LoadLaneKeybinds();
            kBot = new KeyBot();
            kBot.InitHooks();
            SongLoaded = false;
        }

        private void LoadLaneKeybinds()
        {
            laneLayouts.Clear();
            foreach (var pair in DefaultLaneLayouts)
                laneLayouts[pair.Key] = (int[])pair.Value.Clone();

            if (!System.IO.File.Exists(LaneKeybindFile))
            {
                SaveLaneKeybinds();
                return;
            }

            try
            {
                foreach (string rawLine in System.IO.File.ReadAllLines(LaneKeybindFile))
                {
                    if (string.IsNullOrWhiteSpace(rawLine))
                        continue;

                    string[] parts = rawLine.Split('=');
                    if (parts.Length != 2)
                        continue;

                    int keyCount;
                    if (!int.TryParse(parts[0].Trim(), out keyCount) || !DefaultLaneLayouts.ContainsKey(keyCount))
                        continue;

                    string[] values = parts[1].Split(',');
                    if (values.Length != keyCount)
                        continue;

                    var layout = new int[keyCount];
                    bool valid = true;
                    for (int i = 0; i < values.Length; i++)
                    {
                        int key;
                        if (!int.TryParse(values[i].Trim(), out key) || key < 1 || key > 255)
                        {
                            valid = false;
                            break;
                        }
                        layout[i] = key;
                    }

                    if (valid)
                        laneLayouts[keyCount] = layout;
                }
            }
            catch
            {
                foreach (var pair in DefaultLaneLayouts)
                    laneLayouts[pair.Key] = (int[])pair.Value.Clone();
            }
        }

        private void SaveLaneKeybinds()
        {
            try
            {
                var lines = new List<string>();
                for (int keyCount = 1; keyCount <= 18; keyCount++)
                {
                    int[] layout;
                    if (!laneLayouts.TryGetValue(keyCount, out layout))
                        continue;
                    lines.Add(keyCount + "=" + string.Join(",", layout));
                }
                System.IO.File.WriteAllLines(LaneKeybindFile, lines);
            }
            catch
            {
            }
        }

        public int[] GetLaneLayout(int keyCount)
        {
            int[] layout;
            if (!laneLayouts.TryGetValue(keyCount, out layout))
                throw new ArgumentOutOfRangeException(nameof(keyCount));
            return (int[])layout.Clone();
        }

        public void SetLaneKey(int keyCount, int lane, System.Windows.Forms.Keys key)
        {
            int[] layout;
            if (!laneLayouts.TryGetValue(keyCount, out layout) || lane < 0 || lane >= layout.Length)
                return;

            layout[lane] = (int)key;
            SaveLaneKeybinds();
        }

        public void ResetLaneLayout(int keyCount)
        {
            int[] defaults;
            if (!DefaultLaneLayouts.TryGetValue(keyCount, out defaults))
                return;

            laneLayouts[keyCount] = (int[])defaults.Clone();
            SaveLaneKeybinds();
        }

        public void ResetAllLaneLayouts()
        {
            foreach (var pair in DefaultLaneLayouts)
                laneLayouts[pair.Key] = (int[])pair.Value.Clone();
            SaveLaneKeybinds();
        }

        private string FormatTime(TimeSpan t)
        {
            return t.ToString(@"mm\:ss\:fff");
        }

        private void SafeConsole(string text)
        {
            try
            {
                if (!shutdownRequested && Form1.console != null && !Form1.console.IsDisposed)
                    Form1.WriteToConsole(text);
            }
            catch
            {
            }
        }

        private Thread StartWorker(ThreadStart work, ThreadPriority priority = ThreadPriority.Normal)
        {
            var thread = new Thread(() =>
            {
                try
                {
                    Thread.CurrentThread.Priority = priority;
                    work();
                }
                catch (ThreadInterruptedException)
                {
                }
                catch (Exception e)
                {
                    SafeConsole("Worker exception:\n" + e);
                }
                finally
                {
                    lock (threadLock)
                        workerThreads.Remove(Thread.CurrentThread);
                }
            });

            thread.IsBackground = true;
            thread.Priority = priority;

            lock (threadLock)
                workerThreads.Add(thread);

            thread.Start();
            return thread;
        }

        private bool IsSessionCurrent(int generation)
        {
            return !shutdownRequested && generation == Volatile.Read(ref playbackGeneration);
        }

        private void InterruptWorkers()
        {
            Thread[] threads;
            lock (threadLock)
                threads = workerThreads.ToArray();

            foreach (Thread thread in threads)
            {
                if (thread == null || thread == Thread.CurrentThread || !thread.IsAlive)
                    continue;

                try
                {
                    thread.Interrupt();
                }
                catch
                {
                }
            }

            foreach (Thread thread in threads)
            {
                if (thread == null || thread == Thread.CurrentThread || !thread.IsAlive)
                    continue;

                try
                {
                    thread.Join(150);
                }
                catch
                {
                }
            }

            lock (threadLock)
                workerThreads.RemoveAll(t => t == null || !t.IsAlive);
        }

        private void StopPlaybackWorkers()
        {
            Playing = false;
            Interlocked.Increment(ref playbackGeneration);

            if (watch != null)
            {
                try
                {
                    watch.Reset();
                }
                catch
                {
                }
            }

            ReleaseAllKeys();
            InterruptWorkers();
            ReleaseAllKeys();
        }

        private int BuildInputSchedule()
        {
            inputSchedule.Clear();
            var notes = new List<ScheduledNote>();
            maximumSectionHitNotes = 0;

            foreach (FNFSong.FNFSection section in mBot.song.Sections)
            {
                List<FNFSong.FNFNote> sectionNotes = mBot.GetHitNotes(section);
                if (sectionNotes.Count > maximumSectionHitNotes)
                    maximumSectionHitNotes = sectionNotes.Count;

                foreach (FNFSong.FNFNote note in sectionNotes)
                {
                    int lane = mBot.GetLane(note);
                    if (lane < 0 || lane >= mBot.KeyCount)
                        continue;

                    notes.Add(new ScheduledNote
                    {
                        Time = (double)note.Time,
                        Length = (double)note.Length,
                        Lane = lane
                    });
                }
            }

            notes.Sort((a, b) => a.Time.CompareTo(b.Time));

            peakNotesPerSecond = 0;
            int windowStart = 0;
            for (int i = 0; i < notes.Count; i++)
            {
                while (windowStart < i && notes[i].Time - notes[windowStart].Time >= 1000.0)
                    windowStart++;

                int count = i - windowStart + 1;
                if (count > peakNotesPerSecond)
                    peakNotesPerSecond = count;
            }

            minimumLaneGapMs = double.PositiveInfinity;
            var nextLaneTime = new Dictionary<ScheduledNote, double>();
            for (int lane = 0; lane < mBot.KeyCount; lane++)
            {
                List<ScheduledNote> laneNotes = notes.Where(n => n.Lane == lane).OrderBy(n => n.Time).ToList();
                for (int i = 0; i < laneNotes.Count; i++)
                {
                    double nextTime = i + 1 < laneNotes.Count ? laneNotes[i + 1].Time : double.PositiveInfinity;
                    nextLaneTime[laneNotes[i]] = nextTime;

                    if (!double.IsPositiveInfinity(nextTime))
                    {
                        double gap = nextTime - laneNotes[i].Time;
                        if (gap > 0.0001 && gap < minimumLaneGapMs)
                            minimumLaneGapMs = gap;
                    }
                }
            }

            highDensityChart = peakNotesPerSecond >= 24 ||
                               minimumLaneGapMs < 35.0 ||
                               maximumSectionHitNotes >= 32;

            foreach (ScheduledNote note in notes)
            {
                double nextTime = nextLaneTime[note];
                double nextGap = double.IsPositiveInfinity(nextTime)
                    ? double.PositiveInfinity
                    : nextTime - note.Time;

                double duration;
                if (note.Length > 0)
                {
                    duration = Math.Max(1.0, note.Length);
                }
                else if (!double.IsPositiveInfinity(nextGap) && nextGap > 0.0)
                {
                    if (nextGap <= 5.0)
                        duration = Math.Max(1.0, nextGap * 0.45);
                    else
                        duration = Math.Min(18.0, Math.Max(4.0, nextGap - 5.0));
                }
                else
                {
                    duration = 18.0;
                }

                inputSchedule.Add(new ScheduledInputEvent { Time = note.Time, Lane = note.Lane, IsDown = true });
                inputSchedule.Add(new ScheduledInputEvent { Time = note.Time + duration, Lane = note.Lane, IsDown = false });
            }

            inputSchedule.Sort((a, b) =>
            {
                int byTime = a.Time.CompareTo(b.Time);
                if (byTime != 0)
                    return byTime;
                if (a.IsDown == b.IsDown)
                    return 0;
                return a.IsDown ? 1 : -1;
            });

            return notes.Count;
        }

        public void Load(string songDirectory)
        {
            SafeConsole("attempting to load " + songDirectory);

            if (!System.IO.File.Exists(songDirectory))
            {
                SafeConsole("Path doesn't exist");
                SongLoaded = false;
                return;
            }

            StopPlaybackWorkers();
            shutdownRequested = false;
            sngDir = songDirectory;

            try
            {
                mBot = new MapBot(songDirectory);
                rBot = new RenderBot((int)mBot.song.Bpm);
            }
            catch (Exception e)
            {
                SongLoaded = false;
                SafeConsole("Failed to load chart.\n" + e);
                return;
            }

            int hitCount = BuildInputSchedule();

            SongLoaded = hitCount > 0;
            if (!SongLoaded)
                SafeConsole("No hittable notes found in this chart.");

            watch = new Stopwatch();
            int generation = Volatile.Read(ref playbackGeneration);
            currentPlayThread = StartWorker(() => PlayThread(generation), ThreadPriority.Highest);

            string laneGapText = double.IsPositiveInfinity(minimumLaneGapMs)
                ? "n/a"
                : minimumLaneGapMs.ToString("0.###") + " ms";

            SafeConsole(
                "Loaded " + mBot.song.SongName +
                " as " + mBot.KeyCount + "K with " +
                mBot.song.Sections.Count + " sections and " +
                hitCount + " hittable notes. Peak density: " + peakNotesPerSecond +
                " notes/sec; fastest lane gap: " + laneGapText +
                "; max section notes: " + maximumSectionHitNotes + "."
            );

            if (highDensityChart)
                SafeConsole("High-density timing mode enabled: adaptive key pulses + precision scheduler. Note preview is suppressed during playback to protect timing.");

            if (Form1.offset != null)
                Form1.offset.Text = "Offset: " + kBot.offset;
            if (Form1.watchTime != null)
                Form1.watchTime.Text = "Time: 00:00:000";
        }

        private bool PrecisionWaitUntil(double targetMilliseconds, int generation, ref double lastUiUpdate)
        {
            while (IsSessionCurrent(generation) && Playing)
            {
                double elapsed = watch.Elapsed.TotalMilliseconds;
                double remaining = targetMilliseconds - elapsed;
                if (remaining <= 0)
                    return true;

                if (elapsed - lastUiUpdate >= 33.0)
                {
                    lastUiUpdate = elapsed;
                    if (Form1.watchTime != null)
                        Form1.watchTime.Text = "Time: " + FormatTime(watch.Elapsed);
                }

                if (remaining > 8.0)
                {
                    int sleepMs = Math.Max(1, (int)Math.Floor(remaining - 3.0));
                    Thread.Sleep(sleepMs);
                }
                else if (remaining > 2.0)
                {
                    Thread.Sleep(1);
                }
                else if (remaining > 0.5)
                {
                    Thread.Yield();
                }
                else
                {
                    Thread.SpinWait(80);
                }
            }

            return false;
        }

        private void RenderPlayback(int generation)
        {
            foreach (FNFSong.FNFSection section in mBot.song.Sections)
            {
                if (!IsSessionCurrent(generation) || !Playing)
                    return;

                List<FNFSong.FNFNote> notes = mBot.GetHitNotes(section);
                if (notes.Count == 0)
                    continue;

                double target = (double)notes.Min(n => n.Time) - kBot.offset;
                while (IsSessionCurrent(generation) && Playing && watch.Elapsed.TotalMilliseconds < target)
                    Thread.Sleep(5);

                if (!IsSessionCurrent(generation) || !Playing)
                    return;

                rBot.ListNotes(notes);
            }
        }

        private void PlayThread(int generation)
        {
            SafeConsole("Precision play scheduler created...");

            while (IsSessionCurrent(generation))
            {
                if (!Playing)
                {
                    Thread.Sleep(25);
                    continue;
                }

                ReleaseAllKeys();
                watch.Reset();
                watch.Start();

                if (Form1.watchTime != null)
                    Form1.watchTime.Text = "Time: 00:00:000";

                if (Form1.Rendering && mBot.KeyCount == 4 && !highDensityChart)
                    StartWorker(() => RenderPlayback(generation), ThreadPriority.BelowNormal);

                bool cancelled = false;
                double lastUiUpdate = -1000.0;
                double worstLateness = 0.0;
                int[] playbackLayout = GetLaneLayout(mBot.KeyCount);
                timeBeginPeriod(1);

                try
                {
                    foreach (ScheduledInputEvent inputEvent in inputSchedule)
                    {
                        double target = inputEvent.Time - kBot.offset;
                        if (!PrecisionWaitUntil(target, generation, ref lastUiUpdate))
                        {
                            cancelled = true;
                            break;
                        }

                        double lateness = watch.Elapsed.TotalMilliseconds - target;
                        if (lateness > worstLateness)
                            worstLateness = lateness;

                        VirtualKeyCode key = (VirtualKeyCode)playbackLayout[inputEvent.Lane];
                        if (inputEvent.IsDown)
                            AcquireKey(key);
                        else
                            ReleaseKey(key);
                    }
                }
                finally
                {
                    timeEndPeriod(1);
                }

                if (!IsSessionCurrent(generation))
                    break;

                ReleaseAllKeys();

                if (!Playing || cancelled)
                {
                    watch.Reset();
                    if (Form1.watchTime != null)
                        Form1.watchTime.Text = "Time: 00:00:000";
                    continue;
                }

                Playing = false;
                watch.Stop();
                if (Form1.watchTime != null)
                    Form1.watchTime.Text = "Time: " + FormatTime(watch.Elapsed);
                SafeConsole("Completed! Scheduler worst lateness: " + worstLateness.ToString("0.00") + " ms.");
            }
        }
        private VirtualKeyCode GetLaneKey(int keyCount, int lane)
        {
            int[] layout;
            if (!laneLayouts.TryGetValue(keyCount, out layout) || lane < 0 || lane >= layout.Length)
                throw new InvalidOperationException("Unsupported lane " + lane + " for " + keyCount + "K chart.");

            return (VirtualKeyCode)layout[lane];
        }

        private bool AcquireKey(VirtualKeyCode key)
        {
            lock (heldKeyLock)
            {
                int count;
                heldKeys.TryGetValue(key, out count);

                if (count == 0)
                {
                    kBot.KeyDown((int)key);
                }
                else
                {
                    kBot.KeyUp((int)key);
                    kBot.KeyDown((int)key);
                }

                heldKeys[key] = count + 1;
                return true;
            }
        }

        private void ReleaseKey(VirtualKeyCode key)
        {
            lock (heldKeyLock)
            {
                int count;
                if (!heldKeys.TryGetValue(key, out count))
                    return;

                if (count <= 1)
                {
                    try
                    {
                        kBot.KeyUp((int)key);
                    }
                    finally
                    {
                        heldKeys.Remove(key);
                    }
                }
                else
                {
                    heldKeys[key] = count - 1;
                }
            }
        }

        public void ReleaseAllKeys()
        {
            lock (heldKeyLock)
            {
                foreach (VirtualKeyCode key in heldKeys.Keys.ToArray())
                {
                    try
                    {
                        kBot.KeyUp((int)key);
                    }
                    catch
                    {
                    }
                }

                heldKeys.Clear();
            }
        }

        public void Shutdown()
        {
            if (shutdownRequested)
                return;

            shutdownRequested = true;
            SongLoaded = false;
            Playing = false;
            Interlocked.Increment(ref playbackGeneration);

            try
            {
                kBot.StopHooks();
            }
            catch
            {
            }

            ReleaseAllKeys();
            InterruptWorkers();
            ReleaseAllKeys();
        }

        public void Dispose()
        {
            Shutdown();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
        private int notesPlayed;

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

        private Thread StartWorker(ThreadStart work)
        {
            var thread = new Thread(() =>
            {
                try
                {
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

            int hitCount = 0;
            foreach (var sect in mBot.song.Sections)
                hitCount += mBot.GetHitNotes(sect).Count;

            SongLoaded = hitCount > 0;
            if (!SongLoaded)
                SafeConsole("No hittable notes found in this chart.");

            watch = new Stopwatch();
            int generation = Volatile.Read(ref playbackGeneration);
            currentPlayThread = StartWorker(() => PlayThread(generation));

            SafeConsole(
                "Loaded " + mBot.song.SongName +
                " as " + mBot.KeyCount + "K with " +
                mBot.song.Sections.Count + " sections and " +
                hitCount + " hittable notes."
            );

            if (Form1.offset != null)
                Form1.offset.Text = "Offset: " + kBot.offset;
            if (Form1.watchTime != null)
                Form1.watchTime.Text = "Time: 00:00:000";
        }

        private void PlayThread(int generation)
        {
            SafeConsole("Play Thread created...");

            while (IsSessionCurrent(generation))
            {
                if (!Playing)
                {
                    Thread.Sleep(50);
                    continue;
                }

                if (!watch.IsRunning)
                {
                    if (Form1.watchTime != null)
                        Form1.watchTime.Text = "Time: 00:00:000";
                    watch.Reset();
                    watch.Start();
                }

                int sectionSee = 0;
                bool cancelled = false;

                foreach (FNFSong.FNFSection sect in mBot.song.Sections)
                {
                    if (!Playing || !IsSessionCurrent(generation))
                    {
                        cancelled = true;
                        break;
                    }

                    sectionSee++;
                    List<FNFSong.FNFNote> notesToPlay = mBot.GetHitNotes(sect);
                    Interlocked.Exchange(ref notesPlayed, 0);

                    foreach (FNFSong.FNFNote note in notesToPlay)
                    {
                        FNFSong.FNFNote capturedNote = note;
                        StartWorker(() => HandleNote(capturedNote, generation));
                    }

                    if (Form1.Rendering && mBot.KeyCount == 4)
                    {
                        List<FNFSong.FNFNote> capturedNotes = notesToPlay;
                        StartWorker(() =>
                        {
                            if (IsSessionCurrent(generation))
                                rBot.ListNotes(capturedNotes);
                        });
                    }

                    while (Volatile.Read(ref notesPlayed) < notesToPlay.Count && sectionSee == Form1.SectionSee)
                    {
                        if (!Playing || !IsSessionCurrent(generation))
                        {
                            cancelled = true;
                            break;
                        }

                        if (watch.IsRunning && Form1.watchTime != null)
                            Form1.watchTime.Text = "Time: " + FormatTime(watch.Elapsed);

                        Thread.Sleep(1);
                    }

                    if (cancelled)
                        break;

                    if (sectionSee == Form1.SectionSee)
                    {
                        Interlocked.Exchange(ref notesPlayed, 0);
                        sectionSee = 0;
                    }
                }

                if (!IsSessionCurrent(generation))
                    break;

                if (!Playing || cancelled)
                {
                    ReleaseAllKeys();
                    watch.Reset();
                    if (Form1.watchTime != null)
                        Form1.watchTime.Text = "Time: 00:00:000";
                    continue;
                }

                Playing = false;
                if (watch.IsRunning)
                {
                    watch.Stop();
                    if (Form1.watchTime != null)
                        Form1.watchTime.Text = "Time: " + FormatTime(watch.Elapsed);
                }

                ReleaseAllKeys();
                SafeConsole("Completed!");
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
                    kBot.KeyDown((int)key);

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

        private bool WaitUntil(double targetMilliseconds, int generation)
        {
            while (IsSessionCurrent(generation) && Playing && watch.Elapsed.TotalMilliseconds < targetMilliseconds)
                Thread.Sleep(1);

            return IsSessionCurrent(generation) && Playing;
        }

        private void SleepWhileActive(int milliseconds, int generation)
        {
            if (milliseconds <= 0)
                return;

            Stopwatch holdWatch = Stopwatch.StartNew();
            while (holdWatch.ElapsedMilliseconds < milliseconds && IsSessionCurrent(generation) && Playing)
            {
                int remaining = milliseconds - (int)holdWatch.ElapsedMilliseconds;
                Thread.Sleep(Math.Max(1, Math.Min(5, remaining)));
            }
        }

        public void HandleNote(FNFSong.FNFNote note, int generation)
        {
            VirtualKeyCode key = 0;
            bool acquired = false;

            try
            {
                double target = (double)note.Time - kBot.offset;
                if (!WaitUntil(target, generation))
                    return;

                int lane = mBot.GetLane(note);
                key = GetLaneKey(mBot.KeyCount, lane);
                acquired = AcquireKey(key);

                int holdMilliseconds = note.Length > 0
                    ? Math.Max(1, Convert.ToInt32(note.Length))
                    : 25;

                SleepWhileActive(holdMilliseconds, generation);
            }
            finally
            {
                if (acquired)
                    ReleaseKey(key);

                Interlocked.Increment(ref notesPlayed);
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

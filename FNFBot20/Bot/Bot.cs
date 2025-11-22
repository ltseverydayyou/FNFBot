using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using WindowsInput;
using WindowsInput.Native;
using FridayNightFunkin;

namespace FNFBot20
{
    public class Bot
    {
        public static bool Playing = false;
        public static bool SongLoaded = false;

        public static Stopwatch watch { get; set; }

        public string sngDir { get; set; }
        public KeyBot kBot;
        public MapBot mBot;
        public RenderBot rBot;

        public InputSimulator simulator = new InputSimulator();

        public Thread currentPlayThread { get; set; }

        public Bot()
        {
            kBot = new KeyBot();
            kBot.InitHooks();
            SongLoaded = false;
        }

        string FormatTime(TimeSpan t)
        {
            return t.ToString(@"mm\:ss\:fff");
        }

        public void Load(string songDirectory)
        {
            Form1.WriteToConsole("attempting to load " + songDirectory);
            if (!File.Exists(songDirectory))
            {
                Form1.WriteToConsole("Path doesn't exist");
                SongLoaded = false;
                return;
            }

            currentPlayThread?.Abort();
            Form1.currentThreads?.Remove(currentPlayThread);

            sngDir = songDirectory;

            mBot = new MapBot(songDirectory);
            rBot = new RenderBot((int)mBot.song.Bpm);

            int hitCount = 0;
            foreach (var sect in mBot.song.Sections)
                hitCount += mBot.GetHitNotes(sect).Count;

            SongLoaded = hitCount > 0;
            if (!SongLoaded)
                Form1.WriteToConsole("No hittable notes found in this chart.");

            currentPlayThread = new Thread(PlayThread);
            currentPlayThread.Start();

            Form1.currentThreads?.Add(currentPlayThread);

            Form1.WriteToConsole("Loaded " + mBot.song.SongName + " with " + mBot.song.Sections.Count + " sections and " + hitCount + " hittable notes.");

            watch = new Stopwatch();

            Form1.offset.Text = "Offset: " + kBot.offset;
            Form1.watchTime.Text = "Time: 00:00:000";
        }

        int notesPlayed = 0;

        void PlayThread()
        {
            Form1.WriteToConsole("Play Thread created...");
            try
            {
                while (true)
                {
                    if (!watch.IsRunning && Playing)
                    {
                        Form1.watchTime.Text = "Time: 00:00:000";
                        watch.Reset();
                        watch.Start();
                    }
                    else if (!Playing && watch.IsRunning)
                    {
                        Form1.console.Text = "";
                        watch.Reset();
                        Form1.watchTime.Text = "Time: 00:00:000";
                    }

                    if (!Playing)
                    {
                        Thread.Sleep(100);
                        continue;
                    }

                    int sectionSee = 0;

                    foreach (FNFSong.FNFSection sect in mBot.song.Sections)
                    {
                        if (!Playing)
                            break;

                        sectionSee++;
                        List<FNFSong.FNFNote> notesToPlay = mBot.GetHitNotes(sect);

                        notesPlayed = 0;

                        foreach (FNFSong.FNFNote n in notesToPlay)
                        {
                            Thread t = new Thread(() => HandleNote(n));
                            Form1.currentThreads.Add(t);
                            t.Start();
                        }

                        if (!Playing)
                            break;

                        if (Form1.Rendering)
                        {
                            Thread list = new Thread(() => rBot.ListNotes(notesToPlay));
                            Form1.currentThreads.Add(list);
                            list.Start();
                        }

                        while (notesPlayed != notesToPlay.Count && sectionSee == Form1.SectionSee)
                        {
                            if (watch.IsRunning)
                                Form1.watchTime.Text = "Time: " + FormatTime(watch.Elapsed);
                            Thread.Sleep(1);
                            if (!Playing)
                                break;
                        }

                        Form1.WriteToConsole("Section See: " + sectionSee);

                        if (sectionSee == Form1.SectionSee)
                        {
                            notesPlayed = 0;
                            Form1.WriteToConsole("---");
                            sectionSee = 0;
                        }
                    }

                    Form1.console.Text = "";
                    Playing = false;
                    if (watch.IsRunning)
                        Form1.watchTime.Text = "Time: " + FormatTime(watch.Elapsed);
                    Form1.WriteToConsole("Completed!");
                }
            }
            catch (Exception e)
            {
                Form1.WriteToConsole("Exception on Play Thread\n" + e);
            }
        }

        public void HandleNote(FNFSong.FNFNote n)
        {
            double target = (double)n.Time - kBot.offset;

            while (watch.Elapsed.TotalMilliseconds < target)
            {
                Thread.Sleep(1);
                if (!Playing)
                    Thread.CurrentThread.Abort();
            }

            bool shouldHold = n.Length > 0;

            switch (n.Type)
            {
                case FNFSong.NoteType.Left:
                case FNFSong.NoteType.RLeft:
                    if (shouldHold)
                    {
                        simulator.Keyboard.KeyDown(VirtualKeyCode.LEFT);
                        Thread.Sleep(Convert.ToInt32(n.Length));
                        simulator.Keyboard.KeyUp(VirtualKeyCode.LEFT);
                    }
                    else
                    {
                        kBot.KeyPress(0x25, 0x1e);
                    }
                    break;

                case FNFSong.NoteType.Down:
                case FNFSong.NoteType.RDown:
                    if (shouldHold)
                    {
                        simulator.Keyboard.KeyDown(VirtualKeyCode.DOWN);
                        Thread.Sleep(Convert.ToInt32(n.Length));
                        simulator.Keyboard.KeyUp(VirtualKeyCode.DOWN);
                    }
                    else
                    {
                        kBot.KeyPress(0x28, 0x1f);
                    }
                    break;

                case FNFSong.NoteType.Up:
                case FNFSong.NoteType.RUp:
                    if (shouldHold)
                    {
                        simulator.Keyboard.KeyDown(VirtualKeyCode.UP);
                        Thread.Sleep(Convert.ToInt32(n.Length));
                        simulator.Keyboard.KeyUp(VirtualKeyCode.UP);
                    }
                    else
                    {
                        kBot.KeyPress(0x26, 0x11);
                    }
                    break;

                case FNFSong.NoteType.Right:
                case FNFSong.NoteType.RRight:
                    if (shouldHold)
                    {
                        simulator.Keyboard.KeyDown(VirtualKeyCode.RIGHT);
                        Thread.Sleep(Convert.ToInt32(n.Length));
                        simulator.Keyboard.KeyUp(VirtualKeyCode.RIGHT);
                    }
                    else
                    {
                        kBot.KeyPress(0x27, 0x20);
                    }
                    break;
            }

            Interlocked.Increment(ref notesPlayed);
        }
    }
}

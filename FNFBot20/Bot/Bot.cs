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
        }

        private string FormatTime(TimeSpan t)
        {
            return t.ToString(@"mm\:ss\:fff");
        }
        
        public void Load(string songDirectory)
        {
            Form1.WriteToConsole("attempting to load " + songDirectory);
            if (!File.Exists(songDirectory))
            {
                Form1.WriteToConsole("Path doesn't exist");
                return;
            }

            currentPlayThread?.Abort();
            Form1.currentThreads?.Remove(currentPlayThread);

            sngDir = songDirectory;
            
            mBot = new MapBot(songDirectory);
            rBot = new RenderBot((int)mBot.song.Bpm);
            
            currentPlayThread = new Thread(PlayThread);
            currentPlayThread.Start();
            
            Form1.currentThreads?.Add(currentPlayThread);
            
            Form1.WriteToConsole("Loaded "  + mBot.song.SongName + " with " + mBot.song.Sections.Count + " sections.");

            watch = new Stopwatch();
            
            Form1.offset.Text = "Offset: " + kBot.offset;
            Form1.watchTime.Text = "Time: 00:00:000";
        }
        
        private int notesPlayed = 0;
        private void PlayThread()
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
                        sectionSee++;
                        List<FNFSong.FNFNote> notesToPlay = mBot.GetHitNotes(sect);

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
                            Form1.watchTime.Text = "Time: " + FormatTime(watch.Elapsed);
                            Thread.Sleep(1);
                            if (!Playing)
                                break;
                        }

                        if (Form1.DebugEnabled)
                            Form1.WriteToConsole("Finished section " + sectionSee + " with " + notesToPlay.Count + " notes.");

                        if (sectionSee == Form1.SectionSee)
                        {
                            notesPlayed = 0;
                            Form1.WriteToConsole("---");
                            sectionSee = 0;
                        }
                    }
                    Form1.console.Text = "";
                    Playing = false;
                    Form1.WriteToConsole("Completed!");
                    Form1.watchTime.Text = "Time: " + FormatTime(watch.Elapsed);
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
            target -= 10;

            while (watch.Elapsed.TotalMilliseconds < target)
            {
                Thread.Sleep(1);
                if (!Playing)
                    Thread.CurrentThread.Abort();
            }

            if (Form1.DebugEnabled)
                Form1.WriteToConsole("Note: " + n.Type + " @ " + n.Time + " len " + n.Length);

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
                        simulator.Keyboard.KeyDown(VirtualKeyCode.LEFT);
                        Thread.Sleep(40);
                        simulator.Keyboard.KeyUp(VirtualKeyCode.LEFT);
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
                        simulator.Keyboard.KeyDown(VirtualKeyCode.DOWN);
                        Thread.Sleep(40);
                        simulator.Keyboard.KeyUp(VirtualKeyCode.DOWN);
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
                        simulator.Keyboard.KeyDown(VirtualKeyCode.UP);
                        Thread.Sleep(40);
                        simulator.Keyboard.KeyUp(VirtualKeyCode.UP);
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
                        simulator.Keyboard.KeyDown(VirtualKeyCode.RIGHT);
                        Thread.Sleep(40);
                        simulator.Keyboard.KeyUp(VirtualKeyCode.RIGHT);
                    }
                    break;
            }

            notesPlayed++;
        }
    }
}

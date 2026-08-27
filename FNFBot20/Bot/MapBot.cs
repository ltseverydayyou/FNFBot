using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using FridayNightFunkin;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FNFBot20
{
    public class MapBot
    {
        public FNFSong song { get; set; }
        public int KeyCount { get; private set; } = 4;
        public int Mania { get; private set; } = 3;

        public MapBot(string songDir)
        {
            string fixedPath = FixChart(songDir);
            song = new FNFSong(fixedPath);
        }

        private static bool TryReadInt(JToken token, out int value)
        {
            value = 0;
            if (token == null)
                return false;

            if (token.Type == JTokenType.Integer)
            {
                value = token.Value<int>();
                return true;
            }

            return int.TryParse(token.ToString(), out value);
        }

        private void DetectKeyCount(JObject root, JObject songObj)
        {
            int value;

            string[] explicitCountNames =
            {
                "keyCount",
                "keys",
                "playerKeyCount",
                "laneCount",
                "lanes"
            };

            foreach (string name in explicitCountNames)
            {
                if ((songObj != null && TryReadInt(songObj[name], out value)) ||
                    TryReadInt(root[name], out value))
                {
                    KeyCount = Math.Max(1, Math.Min(18, value));
                    Mania = KeyCount - 1;
                    Form1.WriteToConsole("Detected " + KeyCount + "K chart.");
                    return;
                }
            }

            if ((songObj != null && TryReadInt(songObj["mania"], out value)) ||
                TryReadInt(root["mania"], out value))
            {
                Mania = value;
                KeyCount = Math.Max(1, Math.Min(18, value + 1));
                Form1.WriteToConsole("Detected " + KeyCount + "K chart (mania " + value + ").");
                return;
            }

            KeyCount = 4;
            Mania = 3;
            Form1.WriteToConsole("No mania/key-count field found; using 4K.");
        }

        private string FixChart(string path)
        {
            string json;
            try
            {
                json = File.ReadAllText(path);
            }
            catch
            {
                return path;
            }

            JObject root;
            try
            {
                root = JObject.Parse(json);
            }
            catch
            {
                return path;
            }

            JObject songObj = root["song"] as JObject;
            DetectKeyCount(root, songObj);

            var noteTypeDecisions = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);

            if (songObj != null)
            {
                JArray notesArr = songObj["notes"] as JArray;
                if (notesArr != null)
                {
                    foreach (var sectToken in notesArr)
                    {
                        JObject sectObj = sectToken as JObject;
                        if (sectObj == null)
                            continue;

                        JArray sectionNotes = sectObj["sectionNotes"] as JArray;
                        if (sectionNotes == null)
                            continue;

                        var toRemove = new List<JToken>();

                        foreach (var noteToken in sectionNotes)
                        {
                            JArray arr = noteToken as JArray;
                            if (arr == null || arr.Count == 0)
                                continue;

                            string typeName = null;
                            if (arr.Count > 3)
                            {
                                for (int i = 3; i < arr.Count; i++)
                                {
                                    if (arr[i].Type == JTokenType.String)
                                    {
                                        typeName = arr[i].ToString();
                                        break;
                                    }
                                }
                            }

                            if (!string.IsNullOrEmpty(typeName))
                            {
                                bool hitThisType;
                                if (!noteTypeDecisions.TryGetValue(typeName, out hitThisType))
                                {
                                    var result = MessageBox.Show(
                                        "This chart has special notes of type \"" + typeName + "\".\n\n" +
                                        "Should the bot HIT notes of this type?",
                                        "Special Note Type Detected",
                                        MessageBoxButtons.YesNo,
                                        MessageBoxIcon.Question
                                    );

                                    hitThisType = result == DialogResult.Yes;
                                    noteTypeDecisions[typeName] = hitThisType;
                                }

                                if (!hitThisType)
                                {
                                    toRemove.Add(arr);
                                    continue;
                                }
                            }

                            while (arr.Count > 3)
                                arr.RemoveAt(3);
                        }

                        foreach (var rem in toRemove)
                            rem.Remove();
                    }
                }
            }

            string tempPath = Path.Combine(
                Path.GetTempPath(),
                "fnfbot_" + Guid.NewGuid().ToString("N") + "_" + Path.GetFileName(path)
            );

            File.WriteAllText(tempPath, root.ToString(Formatting.None));
            return tempPath;
        }

        public int GetLane(FNFSong.FNFNote note)
        {
            int raw = (int)note.Type;
            if (raw < 0)
                return -1;

            return raw % KeyCount;
        }

        public List<FNFSong.FNFNote> GetHitNotes(FNFSong.FNFSection sect)
        {
            var notes = new List<FNFSong.FNFNote>();

            foreach (FNFSong.FNFNote n in sect.Notes)
            {
                n.Time = Math.Round(n.Time);

                int rawType = (int)n.Type;
                if (rawType < 0 || rawType >= KeyCount * 2)
                    continue;

                bool lowSide = rawType < KeyCount;
                if ((sect.MustHitSection && lowSide) || (!sect.MustHitSection && !lowSide))
                    notes.Add(n);
            }

            return notes;
        }

        public void Compile(string path)
        {
            song.SaveSong(path);
        }
    }
}

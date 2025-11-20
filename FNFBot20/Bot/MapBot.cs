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

        public MapBot(string songDir)
        {
            string fixedPath = FixChart(songDir);
            song = new FNFSong(fixedPath);
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

            var noteTypeDecisions = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);

            JObject songObj = root["song"] as JObject;
            if (songObj != null)
            {
                JArray notesArr = songObj["notes"] as JArray;
                if (notesArr != null)
                {
                    foreach (var sectToken in notesArr)
                    {
                        JObject sectObj = sectToken as JObject;
                        if (sectObj == null) continue;

                        JArray sectionNotes = sectObj["sectionNotes"] as JArray;
                        if (sectionNotes == null) continue;

                        var toRemove = new List<JToken>();

                        foreach (var noteToken in sectionNotes)
                        {
                            JArray arr = noteToken as JArray;
                            if (arr == null || arr.Count == 0) continue;

                            if (arr.Count > 3 && arr[3].Type == JTokenType.String)
                            {
                                string typeName = arr[3].ToString();

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

                                    hitThisType = (result == DialogResult.Yes);
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
                "fnfbot_" + Path.GetFileName(path)
            );

            File.WriteAllText(tempPath, root.ToString(Formatting.None));
            return tempPath;
        }

        public List<FNFSong.FNFNote> GetHitNotes(FNFSong.FNFSection sect)
        {
            List<FNFSong.FNFNote> notes = new List<FNFSong.FNFNote>();
            foreach (FNFSong.FNFNote n in sect.Notes)
            {
                n.Time = Math.Round(n.Time);
                if (sect.MustHitSection && n.Type < (FNFSong.NoteType)4)
                    notes.Add(n);
                else if (n.Type >= (FNFSong.NoteType)4 && !sect.MustHitSection)
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

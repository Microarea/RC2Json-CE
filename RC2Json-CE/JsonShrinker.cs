using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RC2Json
{
    class JsonShrinker
    {
        internal void Compact(string path, bool allControls)
        {
            if (File.Exists(path))
                CompactFile(path, allControls);
            else if (Directory.Exists(path))
            {
                foreach (string file in Directory.GetFiles(path, "*.tbjson", SearchOption.AllDirectories))
                {
                    CompactFile(file, allControls);
                }
            }
            else
            {
                Console.WriteLine("Path not found: " + path);
            }

        }

        private void CompactFile(string file, bool allControls)
        {

            Console.WriteLine("Processing " + file);
            bool updated = false;
            List<MyJObject> removed = new List<MyJObject>();
            MyJObject root = null;
            using (StreamReader reader = new StreamReader(file))
            {
                root = MyJObject.Parse(new JsonTextReader(reader));
                bool isTile = root.GetWndObjType() == WndObjType.Tile;
                if (isTile)
                {
                    if (root["x"] != null)
                    {
                        root["x"] = null;
                        updated = true;
                    }
                    if (root["y"] != null)
                    {
                        root["y"] = null;
                        updated = true;
                    }


                }
                MyJArray ar = root["items"] as MyJArray;
                if (ar == null)
                    return;
                bool radioException = false;
                if (!Anchored(ar))
                    ar.Sort(new MyJObjectComparer());
                foreach (MyJObject child in ar)
                {
                    //controllo ancorato, sono già passato di qui
                    if (!string.IsNullOrEmpty(child["anchor"] as string))
                        continue;
                    string id = child["id"] as string;
                    string controlClass = child["controlClass"] as string;
                    int type = Convert.ToInt32(child.GetWndObjType());
                    if (2 == type && id.Contains("STATIC") && string.IsNullOrEmpty(controlClass))
                    {
                        string text = child["text"] as string;
                        if (string.IsNullOrEmpty(text))
                            continue;

                        if (radioException && text.Equals("To", StringComparison.InvariantCultureIgnoreCase))
                        {
                            radioException = false;
                            continue;
                        }

                        int x = Convert.ToInt32(child["x"]);
                        int y = Convert.ToInt32(child["y"]);
                        MyJObject mjo = Find(child, ar, x, y, allControls);
                        if (mjo != null)
                        {
                            mjo["controlCaption"] = child["text"];
                            mjo["captionFont"] = child["font"];
                            updated = true;
                            removed.Add(child);
                        }
                    }
                    else if (9 == type)
                    {
                        if (!string.IsNullOrEmpty(id) && id.StartsWith("IDC_STATIC_AREA"))
                        {
                            updated = true;
                            root["hasStaticArea"] = true;
                            removed.Add(child);
                        }
                    }
                    else if (10 == type)
                    {
                        string text = child["text"] as string;
                        if (!string.IsNullOrEmpty(text) && text.Equals("From", StringComparison.InvariantCultureIgnoreCase))
                            radioException = true;
                    }
                }
                foreach (var child in removed)
                {
                    ar.Remove(child);
                }

                updated = Anchor(ar) || updated;
            }
            if (updated)
            {
                using (StreamWriter sw = new StreamWriter(file, false, Encoding.UTF8))
                {
                    JsonTextWriter jtw = new JsonTextWriter(sw);
                    jtw.Formatting = Formatting.Indented;
                    root.ToString(jtw);
                    Console.WriteLine("Updated " + file);
                }

                string hjson = Path.ChangeExtension(file, ".hjson");
                if (File.Exists(hjson))
                {
                    string[] lines = File.ReadAllLines(hjson);
                    using (StreamWriter sw = new StreamWriter(hjson, false, Encoding.UTF8))
                    {
                        foreach (string line in lines)
                        {
                            bool found = false;
                            foreach (var child in removed)
                            {
                                string id = child["id"] as string;
                                if (!string.IsNullOrEmpty(id) && Regex.IsMatch(line, "\\s" + id + "\\s"))
                                {
                                    found = true;
                                    break;
                                }
                            }
                            if (!found)
                                sw.WriteLine(line);
                        }

                        Console.WriteLine("Updated " + hjson);
                    }
                }
            }
        }

        private bool Anchored(MyJArray ar)
        {
            foreach (MyJObject child in ar)
                if (!string.IsNullOrEmpty(child["anchor"] as string))
                    return true;
            return false;
        }

        private bool Anchor(MyJArray ar)
        {
            bool updated = false;
            List<MyJObject> anchored = new List<MyJObject>();
            List<Point> points = new List<Point>();
            foreach (MyJObject child in ar)
            {
                if (!string.IsNullOrEmpty(child["anchor"] as string))
                    continue;

                int x = Convert.ToInt32(child["x"]);
                int y = Convert.ToInt32(child["y"]);
                string controlClass = child["controlClass"] as string;
                int type = Convert.ToInt32(child.GetWndObjType());

                if (x == 0)
                {
                    if (10 == type || 11 == type || (2 == type && (string.IsNullOrEmpty(controlClass) || controlClass == "LabelStatic")))
                    {
                        anchored.Add(child);
                        points.Add(new Point(Convert.ToInt32(child["x"]), Convert.ToInt32(child["y"])));
                        child["x"] = null;
                        child["y"] = null;
                        child["anchor"] = "COL1";
                        updated = true;
                    }
                }
                else if (x == 327)
                {
                    if (10 == type || 11 == type || (2 == type && (string.IsNullOrEmpty(controlClass) || controlClass == "LabelStatic")))
                    {
                        anchored.Add(child);
                        points.Add(new Point(Convert.ToInt32(child["x"]), Convert.ToInt32(child["y"])));
                        child["x"] = null;
                        child["y"] = null;
                        child["anchor"] = "COL2";
                        updated = true;
                    }
                }
                else if (x == 101)
                {
                    anchored.Add(child);
                    points.Add(new Point(Convert.ToInt32(child["x"]), Convert.ToInt32(child["y"])));
                    child["x"] = null;
                    child["y"] = null;
                    child["anchor"] = "COL1";
                    updated = true;
                }
                else if (x == 428)
                {
                    anchored.Add(child);
                    points.Add(new Point(Convert.ToInt32(child["x"]), Convert.ToInt32(child["y"])));
                    child["x"] = null;
                    child["y"] = null;
                    child["anchor"] = "COL2";
                    updated = true;
                }

            }
            for (int i = 0; i < anchored.Count; i++)
            {
                AnchorFriend(anchored[i], points[i], points, ar);
            }
            return updated;
        }
        private void AnchorFriend(MyJObject controlToAnchor, Point pt, List<Point> points, MyJArray ar)
        {
            foreach (MyJObject child in ar)
            {
                string id = child["id"] as string;
                if (child != controlToAnchor &&
                    !string.IsNullOrEmpty(id) &&
                    string.Compare(child["activation"] as string, controlToAnchor["activation"] as string) == 0 &&
                    string.IsNullOrEmpty(child["anchor"] as string))
                {
                    Int32 xx = Convert.ToInt32(child["x"]);
                    Int32 yy = Convert.ToInt32(child["y"]);
                    if (Math.Abs(yy - pt.Y) <= 2)//(quasi la) stessa coordinata
                    {
                        //devo controllare che non sia associato ad un altro controllo ancorato (tile wide)
                        foreach (Point otherPt in points)
                            if (!otherPt.Equals(pt) && xx > otherPt.X) //non sono il punto associato al controllo da ancorare, e mi trovo più a destra
                                continue;

                        if (xx >= 327 && pt.X < 327)
                            continue;//si trovano in aree della tile diverse
                        child["x"] = null;
                        child["y"] = null;
                        child["anchor"] = controlToAnchor["id"];
                    }

                }
            }
        }

        private MyJObject Find(MyJObject labelToRemove, MyJArray ar, int x, int y, bool allControls)
        {
            List<MyJObject> candidates = new List<MyJObject>();
            foreach (MyJObject child in ar)
            {
                string id = child["id"] as string;
                string caption = child["controlCaption"] as string;
                if (child != labelToRemove &&
                    !string.IsNullOrEmpty(id) &&
                    !id.StartsWith("IDC_STATIC") &&
                    string.IsNullOrEmpty(caption) &&
                    string.Compare(child["activation"] as string, labelToRemove["activation"] as string) == 0 &&
                    (allControls || !string.IsNullOrEmpty(child["controlClass"] as string)))
                {
                    Int32 yy = Convert.ToInt32(child["y"]);

                    if (Math.Abs(yy - y) < 5)
                    {
                        candidates.Add(child);
                    }

                }
            }
            int min = int.MaxValue;
            MyJObject found = null;
            foreach (MyJObject child in candidates)
            {
                int xx = Convert.ToInt32(child["x"]);
                if (xx < x) //il controllo è a sinistra della label, non può essere la sua
                    continue;
                if ((xx - x) < min)
                {
                    min = xx - x;
                    found = child;
                }
            }
            return found;
        }
    }


}

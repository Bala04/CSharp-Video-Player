using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Video_Player
{
    static class FileAssociationPath
    {
        static String Path;
        static public void setPath(String path)
        {
            Path = path;
        }
        static public string getPath()
        {
            return Path;
        }
    }
}

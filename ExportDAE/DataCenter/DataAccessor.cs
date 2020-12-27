using System;
using System.Windows.Forms;

namespace ExportDAE
{
    internal static class DataAccessor
    {
        public static int InsertionPoint
        {
            get;
            set;
        }

        public static decimal SkipSmallerThan
        {
            get;
            set;
        }

        public static bool SkipInteriorDetails
        {
            get;
            set;
        }

        public static bool CollectTextures
        {
            get;
            set;
        }

        public static bool UnicodeSupport
        {
            get;
            set;
        }

        public static bool GeometryOptimization
        {
            get;
            set;
        }

        public static int levelOfDetail
        {
            get;
            set;
        }
    }

}

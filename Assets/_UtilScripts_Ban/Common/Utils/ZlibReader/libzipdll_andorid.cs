 namespace LibZipInterface
{
    using System;
    using System.Runtime.InteropServices;
    using System.Reflection;
    using System.Collections;
    using System.Text;
    using System.Security;

//#if UNITY_ANDROIDS
  
    [StructLayout(LayoutKind.Sequential)]
    public struct zip_stat {
        public IntPtr   name;			    /* name of the file */
        public int 		index;		        /* index within archive */
        public int crc;			    /* crc of file data */
        public int mtime;			    /* modification time */
        public int size;				/* size of file (uncompressed) */
        public int comp_size;			/* size of file (compressed) */
        public short comp_method;		    /* compression method used */
        public short encryption_method;	/* encryption method used */
    };

 
    public class LibZip
    {
 
        const string ZIPDLL = "zip";
 
        //struct zip *zip_open(const char *, int, int *);
        [DllImport(ZIPDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr zip_open(string strZipPath, int iMode, IntPtr ptrDefaultNull);
       
        //int zip_get_num_files(struct zip *);
        [DllImport(ZIPDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int zip_get_num_files(IntPtr ptrZip);

        //struct zip_file *zip_fopen(struct zip *, const char *, int);
        [DllImport(ZIPDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr zip_fopen(IntPtr ptrZip, string strFileName, int iMode);

        //int zip_stat(struct zip *, const char *, int, struct zip_stat *);
        [DllImport(ZIPDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int zip_stat(IntPtr ptrZip, string strFileName, int iMode, ref zip_stat refZipState);
        
      
        //ssize_t zip_fread(struct zip_file *, void *, size_t);
        [DllImport(ZIPDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int zip_fread(IntPtr ptrZipFile, byte[] ptrBuffer, int iBufferSize);

        //ZIP_EXTERN int zip_fclose(struct zip_file *);
        [DllImport(ZIPDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int zip_fclose(IntPtr ptrZipFile);
        
        //ZIP_EXTERN int zip_close(struct zip *);
        [DllImport(ZIPDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int zip_close(IntPtr ptrZip);

		[DllImport(ZIPDLL, CharSet = CharSet.Ansi,CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr zip_get_name(IntPtr ptrZip,int iFile,int iMode);

    }
//#endif
}
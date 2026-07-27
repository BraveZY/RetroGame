using System;
using System.Runtime.InteropServices;
using System.Reflection;
using System.Collections;
using System.Text;
using System.Security;
using UnityEngine;
using LibZipInterface;

//#if UNITY_ANDROIDS
class ZipFileReader_Andorid
{
    private static IntPtr zip = IntPtr.Zero ;

    public static bool FileExist ( string strZipFilePath )
    {
        if ( zip == IntPtr.Zero )
        {
            zip = LibZip.zip_open( Application.dataPath , 0 , IntPtr.Zero ) ;
        }

        if( zip == IntPtr.Zero )
        {
            ClientLogger.Error( "zip package not found: " + Application.dataPath ) ;
            return false ;
        }

        string filePath = "assets/" + strZipFilePath ;
        IntPtr zipfile = LibZip.zip_fopen( zip , filePath , 0 ) ;

        if( zipfile == IntPtr.Zero )
        {
            ClientLogger.Error( "zip file not found: " + Application.dataPath + "/" + filePath ) ;
            //LibZip.zip_close( zip ) ;
            return false;
        }

        LibZip.zip_fclose( zipfile ) ;
        //LibZip.zip_close( zip ) ;
        return true ;
    }

    public static bool Read ( string strZipFilePath , ref byte[] outBuffer , ref int iBufferSize )
    {  
        outBuffer   = null;
        iBufferSize  = 0;

        if ( zip == IntPtr.Zero )
        {
            zip = LibZip.zip_open( Application.dataPath , 0 , IntPtr.Zero ) ;
        }
  
        if( zip == IntPtr.Zero )
        {
            ClientLogger.Error( "zip package not found: " + Application.dataPath ) ;
            return false;
        }

        string filePath = "assets/" + strZipFilePath ;
        IntPtr zipfile = LibZip.zip_fopen( zip , filePath , 0 ) ;

        if( zipfile == IntPtr.Zero )
        {
            ClientLogger.Error( "zip file not found: " + Application.dataPath + "/" + filePath ) ;
            //LibZip.zip_close(zip);
            return false;
        }

        zip_stat zipfile_stat = new zip_stat();
        LibZip.zip_stat(zip,filePath,0,ref zipfile_stat);
        
        outBuffer = new byte[zipfile_stat.size +1];
        outBuffer[zipfile_stat.size] = 0;
        iBufferSize = zipfile_stat.size + 1;
        LibZip.zip_fread(zipfile, outBuffer,zipfile_stat.size);
  
        LibZip.zip_fclose(zipfile);
        //LibZip.zip_close(zip); 
        return true;
    }
}
//endif

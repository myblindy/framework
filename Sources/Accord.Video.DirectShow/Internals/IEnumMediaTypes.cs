using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Accord.Video.DirectShow.Internals
{
    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
        Guid("89c31040-846b-11ce-97d3-00aa0055595a"),
        InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IEnumMediaTypes
    {
        [PreserveSig]
        int Next(
            [In] int cMediaTypes,
            [In, Out] ref AMMediaType ppMediaTypes,
            [Out] out int pcFetched
            );

        [PreserveSig]
        int Skip([In] int cMediaTypes);

        [PreserveSig]
        int Reset();

        [PreserveSig]
        int Clone([Out] out IntPtr ppEnum);
    }
}

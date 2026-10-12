using System.Runtime.InteropServices;
namespace BurebistaTrapperDog;

// A private PCM stream: independent of Unity's AudioListener and other mods' sounds.
internal sealed class DogAudio : IDisposable {
 [StructLayout(LayoutKind.Sequential, Pack=2)] struct Format { public ushort tag,channels;public uint rate,bytes;public ushort align,bits,extra; }
 [StructLayout(LayoutKind.Sequential)] struct Header { public IntPtr data;public uint length,recorded;public UIntPtr user;public uint flags,loops;public IntPtr next;public UIntPtr reserved; }
 [DllImport("winmm.dll")] static extern uint waveOutOpen(out IntPtr h,uint device,ref Format format,IntPtr callback,IntPtr instance,uint flags);
 [DllImport("winmm.dll")] static extern uint waveOutPrepareHeader(IntPtr h,IntPtr header,uint size);
 [DllImport("winmm.dll")] static extern uint waveOutWrite(IntPtr h,IntPtr header,uint size);
 [DllImport("winmm.dll")] static extern uint waveOutUnprepareHeader(IntPtr h,IntPtr header,uint size);
 [DllImport("winmm.dll")] static extern uint waveOutReset(IntPtr h);
 [DllImport("winmm.dll")] static extern uint waveOutClose(IntPtr h);
 IntPtr handle,data,header;bool prepared;static readonly uint Size=(uint)Marshal.SizeOf<Header>();
 public bool Busy { get { Poll();return handle!=IntPtr.Zero; } }
 public void Poll(){if(header!=IntPtr.Zero&&(Marshal.PtrToStructure<Header>(header).flags&1)!=0)Dispose();}
 static void Check(uint code){if(code!=0)throw new IOException("Salida de audio Windows: "+code);}
 public void Play(string path,float volume){
  if(Busy)return;
  using var reader=new BinaryReader(File.OpenRead(path));
  if(new string(reader.ReadChars(4))!="RIFF")throw new InvalidDataException("WAV RIFF requerido");
  reader.ReadUInt32();if(new string(reader.ReadChars(4))!="WAVE")throw new InvalidDataException("WAVE requerido");
  Format format=default;byte[] pcm=null;
  while(reader.BaseStream.Position+8<=reader.BaseStream.Length){
   string id=new string(reader.ReadChars(4));uint length=reader.ReadUInt32();long end=reader.BaseStream.Position+length;
   if(end>reader.BaseStream.Length)throw new InvalidDataException("WAV truncado");
   if(id=="fmt "&&length>=16){format.tag=reader.ReadUInt16();format.channels=reader.ReadUInt16();format.rate=reader.ReadUInt32();format.bytes=reader.ReadUInt32();format.align=reader.ReadUInt16();format.bits=reader.ReadUInt16();}
   else if(id=="data")pcm=reader.ReadBytes(checked((int)length));
   reader.BaseStream.Position=end+(length&1);
  }
  if(format.tag!=1||format.bits!=16||format.channels<1||format.channels>2||format.align!=format.channels*2||format.rate==0||format.bytes!=format.rate*format.align||pcm==null||pcm.Length==0||pcm.Length%format.align!=0)throw new InvalidDataException("Se requiere WAV PCM de 16 bits");
  volume=Math.Clamp(volume,0,1);
  for(int i=0;i<pcm.Length;i+=2){short sample=(short)(BitConverter.ToInt16(pcm,i)*volume);pcm[i]=(byte)sample;pcm[i+1]=(byte)(sample>>8);}
  try{
   Check(waveOutOpen(out handle,uint.MaxValue,ref format,IntPtr.Zero,IntPtr.Zero,0));
   data=Marshal.AllocHGlobal(pcm.Length);Marshal.Copy(pcm,0,data,pcm.Length);
   header=Marshal.AllocHGlobal((int)Size);Marshal.StructureToPtr(new Header{data=data,length=(uint)pcm.Length},header,false);
   Check(waveOutPrepareHeader(handle,header,Size));prepared=true;Check(waveOutWrite(handle,header,Size));
  }catch{Dispose();throw;}
 }
 public void Dispose(){
  if(handle!=IntPtr.Zero){waveOutReset(handle);if(prepared)waveOutUnprepareHeader(handle,header,Size);waveOutClose(handle);}
  if(header!=IntPtr.Zero)Marshal.FreeHGlobal(header);if(data!=IntPtr.Zero)Marshal.FreeHGlobal(data);
  handle=data=header=IntPtr.Zero;prepared=false;
 }
}

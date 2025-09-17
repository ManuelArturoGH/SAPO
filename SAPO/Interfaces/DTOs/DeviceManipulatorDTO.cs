namespace BioMetrixCore.DTOs
{
    public interface DeviceManipulatorDTO
    {
        bool PingDevice(string ipAddress);
        bool ConnectDevice(string ipAddress);
        
    }
}
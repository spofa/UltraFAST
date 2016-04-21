using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TravelObjects;

namespace RemoteClient
{
    public interface IService
    {
        void OnData(TransferData data);
        void start();

        void stop();
        void SendImage();

        void SendSettings();
    }
}

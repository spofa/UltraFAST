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
        void OnData(Data data);
        void start();

        void stop();

    }
}

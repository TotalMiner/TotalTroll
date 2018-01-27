using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace TotalTroll.Services
{
    public class ThankService
    {
        private string _path;
        private List<string> _thankList;
        private Dictionary<ulong, Bucket> _bucketList;

        public ThankService(string path)
        {
            _path = path;
            _thankList = new List<string>(File.ReadAllLines(path));
            _bucketList = new Dictionary<ulong, Bucket>();
        }

        public List<string> GetList() => _thankList;

        public bool ContainsThank(string str) => _thankList.FindAll(s => str.StartsWith(s)).Count > 0;

        public bool IsThank(string thank) => _thankList.Contains(thank);

        public int Allowance(SocketUser user) => Allowance(user.Id);

        public int Allowance(ulong id) => _Contains(id) ? _Allowance(id) : _AllowanceFromNew(id);

        public double WaitTime(SocketUser user) => WaitTime(user.Id);

        public double WaitTime(ulong id) => _Contains(id) ? _WaitTime(id) : _WaitTimeFromNew(id);

        public bool CanThank(SocketUser user, int times) => Allowance(user.Id) >= times;

        public bool CanThank(ulong id, int times) => Allowance(id) >= times;

        public bool CanThank(SocketUser user) => CanThank(user, 1);

        public bool CanThank(ulong id) => CanThank(id, 1);

        private bool _Contains(ulong id) => _bucketList.ContainsKey(id);

        private void _NewUser(ulong id)
        {
            _bucketList.Add(id, new Bucket(5, 3600));
        }

        private double _WaitTime(ulong id)
        {
            return _bucketList[id].WaitTime();
        }

        private double _WaitTimeFromNew(ulong id)
        {
            _NewUser(id);
            return _WaitTime(id);
        }

        private int _Allowance(ulong id)
        {
            return (int)Math.Floor(_bucketList[id].Allowance());
        }

        private int _AllowanceFromNew(ulong id)
        {
            _NewUser(id);
            return _Allowance(id);
        }
    }
}

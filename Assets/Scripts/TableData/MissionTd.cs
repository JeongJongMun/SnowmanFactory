using UnityEngine;
using System.Collections.Generic;

namespace Core.Scripts.Table
{
    [System.Serializable]
    public class MissionTdData
    {
        [SerializeField]
		private int _id;
		public int id
		{
			get { return _id;}
			set { _id = value;}
		}
		[SerializeField]
		private int _time;
		public int time
		{
			get { return _time;}
			set { _time = value;}
		}
		[SerializeField]
		private int _goal;
		public int goal
		{
			get { return _goal;}
			set { _goal = value;}
		}

    }

    [System.Serializable]
    public class MissionTd : Table<MissionTdData, int>
    {
    }
}
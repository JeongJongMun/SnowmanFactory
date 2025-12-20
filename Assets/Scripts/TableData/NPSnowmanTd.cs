using UnityEngine;
using System.Collections.Generic;

namespace Core.Scripts.Table
{
    [System.Serializable]
    public class NPSnowmanTdData
    {
        [SerializeField]
		private int _id;
		public int id
		{
			get { return _id;}
			set { _id = value;}
		}
		[SerializeField]
		private string _upperbody_animator_name;
		public string upperbody_animator_name
		{
			get { return _upperbody_animator_name;}
			set { _upperbody_animator_name = value;}
		}
		[SerializeField]
		private string _fullbody_animator_name;
		public string fullbody_animator_name
		{
			get { return _fullbody_animator_name;}
			set { _fullbody_animator_name = value;}
		}

    }

    [System.Serializable]
    public class NPSnowmanTd : Table<NPSnowmanTdData, int>
    {
    }
}
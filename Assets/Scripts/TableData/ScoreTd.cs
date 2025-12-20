using UnityEngine;
using System.Collections.Generic;

namespace Core.Scripts.Table
{
    [System.Serializable]
    public class ScoreTdData
    {
        [SerializeField]
		private int _id;
		public int id
		{
			get { return _id;}
			set { _id = value;}
		}
		[SerializeField]
		private string _action_type;
		public string action_type
		{
			get { return _action_type;}
			set { _action_type = value;}
		}
		[SerializeField]
		private int _score;
		public int score
		{
			get { return _score;}
			set { _score = value;}
		}

    }

    [System.Serializable]
    public class ScoreTd : Table<ScoreTdData, int>
    {
        public int GetScore(string actionType)
        {
            foreach (var data in DataList)
            {
                if (data.action_type == actionType)
                    return data.score;
            }
            return 0;
        }
    }
}
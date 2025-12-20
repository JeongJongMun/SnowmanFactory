using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace Core.Scripts.Table
{
    [System.Serializable]
    public class RankTdData
    {
        [SerializeField]
		private int _id;
		public int id
		{
			get { return _id;}
			set { _id = value;}
		}
		[SerializeField]
		private string _rank;
		public string rank
		{
			get { return _rank;}
			set { _rank = value;}
		}
		[SerializeField]
		private int _min_score;
		public int min_score
		{
			get { return _min_score;}
			set { _min_score = value;}
		}
		[SerializeField]
		private int _max_defect_pass;
		public int max_defect_pass
		{
			get { return _max_defect_pass;}
			set { _max_defect_pass = value;}
		}

    }

    [System.Serializable]
    public class RankTd : Table<RankTdData, int>
    {
        public string GetRank(int totalScore, int defectPassCount, bool missionSuccess)
        {
            if (!missionSuccess) return "F";

            var sortedRanks = DataList
                .Where(x => x.rank != "F")
                .OrderByDescending(x => x.min_score)
                .ToList();

            foreach (var rankData in sortedRanks)
            {
                if (totalScore >= rankData.min_score &&
                    defectPassCount <= rankData.max_defect_pass)
                {
                    return rankData.rank;
                }
            }

            return "F";
        }
    }
}
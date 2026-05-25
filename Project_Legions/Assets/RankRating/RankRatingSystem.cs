using UnityEngine;

namespace PPCorps
{
    public static class RankRatingSystem
    {
        public static int GetRankSegment(int rating)
        {
            if (rating <= 1000) return 1;
            if (rating <= 2000) return 2;
            return 3;
        }

        public static int CalcWinGain(int myRating, int opponentRating)
        {
            int diff = opponentRating - myRating;

            if (diff <= 150)
                return 10;
            else if (diff <= 2000)
            {
                return Mathf.RoundToInt(10f + (diff - 150) * 30f / 1850f);
            }
            else
            {
                return 40;
            }
        }

        public static int CalcLossChange(int myRating, int opponentRating)
        {
            int mySeg = GetRankSegment(myRating);
            int oppSeg = GetRankSegment(opponentRating);
            int diff = myRating - opponentRating;

            float t = Mathf.Clamp01((diff + 300f) / 600f);

            int minLoss, maxLoss;

            if (myRating < 500)
            {
                if (oppSeg == 3)
                {
                    minLoss = 2; maxLoss = 2;
                }
                else
                {
                    minLoss = 2; maxLoss = 5;
                }
            }
            else if (myRating <= 1000)
            {
                if (opponentRating <= 500)
                {
                    minLoss = 5; maxLoss = 10;
                }
                else if (opponentRating <= 1000)
                {
                    minLoss = 3; maxLoss = 8;
                }
                else
                {
                    minLoss = 2; maxLoss = 3;
                }
            }
            else if (mySeg == 2)
            {
                if (oppSeg == 1)
                {
                    minLoss = 5; maxLoss = 12;
                }
                else if (oppSeg == 2)
                {
                    minLoss = 2; maxLoss = 8;
                }
                else
                {
                    minLoss = 2; maxLoss = 4;
                }
            }
            else
            {
                if (oppSeg == 1)
                {
                    minLoss = 10; maxLoss = 15;
                }
                else if (oppSeg == 2)
                {
                    minLoss = 5; maxLoss = 10;
                }
                else
                {
                    minLoss = 2; maxLoss = 10;
                }
            }

            int loss = Mathf.RoundToInt(Mathf.Lerp(minLoss, maxLoss, t));
            loss = Mathf.Clamp(loss, minLoss, maxLoss);
            return -loss;
        }
    }
}

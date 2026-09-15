using System;
namespace HundredHour.UI.Timeline
{
    public sealed class TimelinePaginationModel
    {
        public int TotalCount {get;private set;}
        public int PageSize {get;private set;}
        public int PageIndex {get;private set;}
        public int PageCount=>TotalCount==0?0:(TotalCount-1)/PageSize+1;
        public int StartIndex=>PageIndex*PageSize;
        public int VisibleCount=>Math.Min(PageSize,Math.Max(0,TotalCount-StartIndex));
        public bool HasNext=>PageIndex+1<PageCount;
        public void Reset(int totalCount,int pageSize)
        {TotalCount=Math.Max(0,totalCount);PageSize=Math.Max(1,pageSize);PageIndex=0;}
        public bool Next(){if(!HasNext)return false;PageIndex++;return true;}
    }
}

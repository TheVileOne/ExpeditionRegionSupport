namespace LogUtils.Enums
{
    public interface ICombiner<TCombinable, TComposite>
    {
        TComposite Combine(TCombinable a, TCombinable b);
        TComposite Distinct(TCombinable a, TCombinable b);
        TCombinable GetComplement(TCombinable target);
        TComposite Intersect(TCombinable a, TCombinable b);
    }
}

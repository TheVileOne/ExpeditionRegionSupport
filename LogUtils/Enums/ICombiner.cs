using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

using System;
using System.Threading.Tasks;
using Microsoft.FeatureManagement;

namespace Yotalab.PlanningPoker.BlazorServerSide.Filters
{
  public class SnowflakesFeatureFilter : IFeatureFilter
  {
    private readonly DateTime startPeriod;
    private readonly DateTime endPeriod;

    public SnowflakesFeatureFilter()
    {
      this.startPeriod = new DateTime(DateTime.Now.Year, 12, 1);
      this.endPeriod = startPeriod.AddDays(45);
    }

    public Task<bool> EvaluateAsync(FeatureFilterEvaluationContext context)
    {
      var now = DateTime.Now;
      return Task.FromResult(now >= this.startPeriod && now <= this.endPeriod);
    }
  }
}

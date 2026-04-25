
namespace ALOud.Models;


public class JobsConfig
{

    public class IndexingJobConfig
    {
        public bool OnStartIndexing { get; }
        public int DelaySeconds { get; }

        public IndexingJobConfig()
        {
            OnStartIndexing = false;
            DelaySeconds = 5;
        }
    }

}
public class Rand {
    private static System.Random rnd = null;
    private static double lastValue = 0;

    public System.Random random { get { return rnd; } }

    public int Next (int limit) {
        if (rnd != null) {
                int tmp = rnd.Next (limit);
                /* if (lastValue == tmp)
                    lastValue = rnd.Next (limit);
                else */ lastValue = tmp;
        }
        return (int) lastValue;
    }
    public double NextDouble () {
        if (rnd != null) {
                double tmp = rnd.NextDouble ();
                /* if (lastValue == tmp)
                    lastValue = rnd.NextDouble ();
                else  */lastValue = tmp;
        }
        return lastValue;
    }
    public Rand () {
        if (rnd == null)
            rnd = new System.Random ();
    }
}
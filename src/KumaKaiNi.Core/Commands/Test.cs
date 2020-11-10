namespace KumaKaiNi.Core
{
    [RequireAdmin]
    public static class Test
    {
        [Command("test")]
        public static Response CreateTable()
        {
            return new Response("done");
        }
    }
}

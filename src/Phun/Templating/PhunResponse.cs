namespace Phun.Templating
{
    using System.Web;

    /// <summary>
    /// Phun response.
    /// </summary>
    public class PhunResponse : IResponse
    {                        
        private readonly HttpContextBase context;

        /// <summary>
        /// Initializes a new instance of the <see cref="PhunResponse"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        public PhunResponse(HttpContextBase context)
        {
            this.context = context;
        }

        /// <summary>
        /// Writes the specified chars.
        /// </summary>
        /// <param name="chars">The chars.</param>
        public void write(string[] chars)
        {
            if (chars == null || chars.Length <= 0)
            {
                return;
            }

            foreach (string t in chars)
            {
                this.context.Response.Write(t);
            }
        }
    }
}

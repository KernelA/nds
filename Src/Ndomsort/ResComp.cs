// This is an open source non-commercial project. Dear PVS-Studio, please check it. PVS-Studio Static
// Code Analyzer for C, C++ and C#: http://www.viva64.com
namespace Nds
{
    internal class ConverterResCmp
    {
        /// <summary>
        /// Convert <paramref name="ResultCmp"/> to the <see cref="ResComp"/>. 
        /// </summary>
        /// <param name="ResultCmp"></param>
        /// <returns></returns>
        public static ResComp ConvertToResCmp(int ResultCmp)
        {
            if (ResultCmp < 0)
            {
                return ResComp.LE;
            }
            else if (ResultCmp > 0)
            {
                return ResComp.GR;
            }
            else
            {
                return ResComp.EQ;
            }
        }
    }

    /// <summary>
    /// <para> The result of the comparison. </para>
    /// <list type="bullet">
    /// <listheader>
    /// <term> Description </term>
    /// </listheader>
    /// <item>
    /// <term> <see cref="LE"/> </term>
    /// <description> Less. </description>
    /// </item>
    /// <item>
    /// <term> <see cref="EQ"/> </term>
    /// <description> Equal. </description>
    /// </item>
    /// <item>
    /// <term> <see cref="GR"/> </term>
    /// <description> Greater. </description>
    /// </item>
    /// </list>
    /// </summary>
    internal enum ResComp { LE, EQ, GR };
}
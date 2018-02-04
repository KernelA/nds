// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
namespace Nds
{
    using System;

    /// <summary>
    /// <para>
    /// The result of the comparison, which returns from the <see cref="IComparable{T}.CompareTo(T)"/>.
    /// </para>
    /// <list type="bullet">
    /// <listheader>
    /// <term>Description</term>
    /// </listheader>
    /// <item>
    /// <term><see cref="LE"/></term>
    /// <description>Less.</description>
    /// </item>
    /// <item>
    /// <term><see cref="EQ"/></term>
    /// <description>Equal.</description>
    /// </item>
    /// <item>
    /// <term><see cref="GR"/></term>
    /// <description>Greater.</description>
    /// </item>
    /// </list>
    /// </summary>
    internal enum ResComp { LE = -1, EQ, GR};
}

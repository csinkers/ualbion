using System;
using System.Collections.Generic;

namespace UAlbion.Formats
{
    public static class XDelta
    {
        /* J. MacDonald. File system support for delta compression. MSThesis, UCBerkeley, May 2000 http://pop.xmailserver.net/xdfs.pdf

        s = fingerprint width (16)
        computeDelta(src, tgt)
            i ← 0
            sindex ← initMatch(src) // Initialise string matching
            while (i < size(tgt)) // Loop over target offsets
                (o, l) ← findMatch(src, sindex, tgt, i) // Find longest match
                if (l < s)
                    outputInst({insert tgt[i]}) // Insert instruction
                else
                    outputInst({copy o l}) // Copy instruction
                i ← i + 1            

        initMatch(src)
            i ← 0
            sindex ← empty // Init output array (hash table)
            while (i + s ≤ size(src)) // Loop over source blocks
                f ← adler32(src, i, i + s) // Compute fingerprint
                sindex[hash(f)] ← i // Enter in table
                i ← i + s
            return (sindex)

        findMatch(src, sindex, tgt, o_tgt)
            f ← adler32(tgt, o_tgt, o_tgt + s) // Compute fingerprint
            if (sindex[hash(f)] = nil)
                return (-1, -1) // No match found
            o_src ← sindex[hash(f)]
            l ← matchLength(tgt, o_tgt, src, o_src) // Compute match length
            return (o_src, l)

        adler32(tgt, from, to)) // https://www.ietf.org/rfc/rfc1950.txt
            s1 ← 1
            s2 ← 0
            for (i = from; i < to; i++)
            {
                s1 ← s1 + tgt[i]
                if (s1 ≥ 65521)
                    s1 ← s1 - 65521
                
                s2 ← s2 + s1;
                if (s2 ≥ 65521)
                    s2 ← s2 - 65521

            }
            return s2 × 65536 + s1
        */

        const int FingerprintWidth = 16;

        public static IEnumerable<DiffOperation> Compare(byte[] source, byte[] target)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (target == null) throw new ArgumentNullException(nameof(target));

            var sindex = InitMatch(source); // Initialise string matching
            for (int i = 0; i < target.Length;) // Loop over target offsets
            {
                var (offset, length) = FindMatch(source, sindex, target, i); // Find longest match
                if (length < FingerprintWidth)
                {
                    yield return DiffOperation.Insert(target[i]);
                    i++;
                }
                else
                {
                    yield return DiffOperation.Copy(offset, length);
                    i += length;
                }
            }
        }

        static Dictionary<uint, int> InitMatch(byte[] source)
        {
            var hashes = new Dictionary<uint, int>(); // Init output array (hash table)
            for (int i = 0; i + FingerprintWidth <= source.Length; i += FingerprintWidth) // Loop over source blocks
            {
                var hash = Adler32(source, i, i + FingerprintWidth); // Compute fingerprint
                if (!hashes.ContainsKey(hash)) // Keep first match rather than last
                    hashes[hash] = i; // Enter in table
            }
            return hashes;
        }

        static (int, int) FindMatch(byte[] source, Dictionary<uint, int> sindex, byte[] target, int targetOffset)
        {
            if(targetOffset + FingerprintWidth >= target.Length)
                return (-1, -1);

            var f = Adler32(target, targetOffset, targetOffset + FingerprintWidth); // Compute fingerprint
            if (!sindex.TryGetValue(f, out var sourceOffset))
                return (-1, -1); // No match found

            var l = MatchLength(target, targetOffset, source, sourceOffset); // Compute match length
            return (sourceOffset, l);
        }

        static int MatchLength(byte[] target, int targetOffset, byte[] source, int sourceOffset)
        {
            int delta = targetOffset - sourceOffset;
            int i = targetOffset;
            while (i < target.Length && i - delta < source.Length && target[i] == source[i - delta])
                i++;
            return i - targetOffset;
        }

        static uint Adler32(byte[] target, int from, int to) // https://www.ietf.org/rfc/rfc1950.txt
        {
            int s1 = 1;
            int s2 = 0;
            for (int i = from; i < to; i++)
            {
                s1 += target[i];
                if (s1 >= 65521)
                    s1 -= 65521;
                
                s2 += s1;
                if (s2 >= 65521)
                    s2 -= 65521;
            }

            return (uint)(s2 * 65536 + (long)s1);
        }
    }
}

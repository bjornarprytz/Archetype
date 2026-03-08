using Archetype.Core;

namespace Archetype.Engine;

/// <summary>
/// Engine-owned deterministic pseudo-random number generator implementing the
/// <b>xoshiro128**</b> algorithm (Blackman &amp; Vigna, 2018).
/// <para>
/// xoshiro128** is a 128-bit state generator with a 2^128−1 period.  It is
/// fast, passes all known statistical tests, and — critically — is completely
/// independent of <c>System.Random</c>, whose algorithm changed in .NET 6 and
/// is not guaranteed stable in future versions.  Using an engine-owned
/// algorithm ensures that a <see cref="RngSnapshot"/> replayed on any .NET
/// version produces identical sequences.
/// </para>
/// <para>
/// <b>Algorithm reference:</b>
/// https://prng.di.unimi.it/xoshiro128starstar.c
/// Public domain.
/// </para>
/// </summary>
public sealed class SeededRandom : IRandomSource
{
    // xoshiro128** state — four 32-bit words.
    private uint _s0, _s1, _s2, _s3;

    // Monotonic counter incremented on every NextInt call — stored in
    // RngSnapshot so the load path can fast-forward to the same position.
    private long _callCount;

    // The original seed — stored so Snapshot() can recreate a RngSnapshot
    // without needing the caller to track it.
    private readonly long _seed;

    // -----------------------------------------------------------------------
    //  Construction
    // -----------------------------------------------------------------------

    /// <summary>
    /// Constructs a new <see cref="SeededRandom"/> from a 64-bit seed.
    /// Uses splitmix64 to expand the seed into the four 32-bit state words.
    /// </summary>
    /// <param name="seed">The seed value.  The same seed always produces the same sequence.</param>
    public SeededRandom(long seed)
    {
        _seed = seed;
        // Use splitmix64 to derive four independent 32-bit words from the seed.
        // splitmix64 is the recommended seeding method for xoshiro generators.
        ulong z = (ulong)seed;
        _s0 = (uint)(Splitmix64(ref z) >> 32);
        _s1 = (uint)(Splitmix64(ref z) >> 32);
        _s2 = (uint)(Splitmix64(ref z) >> 32);
        _s3 = (uint)(Splitmix64(ref z) >> 32);

        // Guarantee non-zero state (splitmix64 never produces zero output for
        // non-degenerate inputs, but be defensive).
        if (_s0 == 0 && _s1 == 0 && _s2 == 0 && _s3 == 0)
            _s0 = 1;
    }

    /// <summary>
    /// Constructs a <see cref="SeededRandom"/> in the state it would be in
    /// after <paramref name="callCount"/> calls from <paramref name="seed"/>.
    /// Used by the D17 load path to restore the RNG position from a snapshot.
    /// </summary>
    private SeededRandom(long seed, long callCount) : this(seed)
    {
        // Advance the generator callCount steps.  O(callCount) — acceptable
        // at card-game scale (hundreds of calls per session).
        // xoshiro128** supports an O(1) jump table for large skips if this
        // ever becomes a bottleneck.
        for (long i = 0; i < callCount; i++)
            NextRaw();

        _callCount = callCount;
    }

    /// <summary>
    /// Factory: constructs a <see cref="SeededRandom"/> fast-forwarded to
    /// the position described by <paramref name="snapshot"/>.
    /// </summary>
    public static SeededRandom FromSnapshot(RngSnapshot snapshot) =>
        new(snapshot.Seed, snapshot.CallCount);

    // -----------------------------------------------------------------------
    //  IRandomSource implementation
    // -----------------------------------------------------------------------

    /// <inheritdoc/>
    /// <remarks>
    /// Uses rejection sampling to produce a uniform value in
    /// <c>[minInclusive, maxInclusive]</c> without modulo bias.
    /// </remarks>
    public int NextInt(int minInclusive, int maxInclusive)
    {
        if (minInclusive > maxInclusive)
            throw new ArgumentOutOfRangeException(nameof(minInclusive),
                "minInclusive must be ≤ maxInclusive.");

        if (minInclusive == maxInclusive) return minInclusive;

        uint range = (uint)(maxInclusive - minInclusive) + 1;

        // Rejection sampling: find the largest multiple of `range` that fits
        // in uint, then reject values above it to eliminate modulo bias.
        uint threshold = (uint.MaxValue - range + 1) % range;

        // _callCount tracks the total number of raw generator steps (NextRaw
        // invocations).  The fast-forward constructor advances by exactly
        // _callCount raw steps to reproduce the identical state.  Rejection
        // samples are counted so the replay is bit-for-bit deterministic.
        uint raw;
        do
        {
            raw = NextRaw();
            _callCount++; // count every raw step for deterministic replay
        } while (raw < threshold);

        return minInclusive + (int)(raw % range);
    }

    /// <inheritdoc/>
    /// <remarks>Uses Fisher-Yates shuffle driven by <see cref="NextInt"/>.</remarks>
    public void Shuffle<T>(IList<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = NextInt(0, i);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

    // -----------------------------------------------------------------------
    //  Snapshot
    // -----------------------------------------------------------------------

    /// <summary>
    /// Returns an <see cref="RngSnapshot"/> capturing the current seed and
    /// call count.  The snapshot is sufficient to reconstruct this generator
    /// at its exact current state via <see cref="FromSnapshot"/>.
    /// </summary>
    public RngSnapshot Snapshot() => new(_seed, _callCount);

    // -----------------------------------------------------------------------
    //  xoshiro128** core
    // -----------------------------------------------------------------------

    /// <summary>
    /// Produces the next raw 32-bit value from the xoshiro128** generator
    /// and advances the state.  Does NOT increment <c>_callCount</c> — the
    /// public methods are responsible for that so fast-forward replay works
    /// correctly.
    /// </summary>
    private uint NextRaw()
    {
        // xoshiro128**: result = rotl(s1 * 5, 7) * 9
        uint result = RotL(_s1 * 5, 7) * 9;

        // State update
        uint t = _s1 << 9;
        _s2 ^= _s0;
        _s3 ^= _s1;
        _s1 ^= _s2;
        _s0 ^= _s3;
        _s2 ^= t;
        _s3 = RotL(_s3, 11);

        return result;
    }

    private static uint RotL(uint x, int k) => (x << k) | (x >> (32 - k));

    // -----------------------------------------------------------------------
    //  splitmix64 — seed expansion helper
    // -----------------------------------------------------------------------

    /// <summary>
    /// One step of the splitmix64 algorithm.  Advances the 64-bit state
    /// <paramref name="z"/> and returns a 64-bit output.
    /// Used to expand a single seed long into four independent state words.
    /// </summary>
    private static ulong Splitmix64(ref ulong z)
    {
        z += 0x9E3779B97F4A7C15UL;
        ulong result = z;
        result = (result ^ (result >> 30)) * 0xBF58476D1CE4E5B9UL;
        result = (result ^ (result >> 27)) * 0x94D049BB133111EBUL;
        return result ^ (result >> 31);
    }
}

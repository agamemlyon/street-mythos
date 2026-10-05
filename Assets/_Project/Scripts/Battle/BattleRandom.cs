namespace StreetMythos.Battle
{
    // Générateur déterministe à graine fixe (xorshift32) : un combat rejoué donne le même résultat
    public sealed class BattleRandom
    {
        uint _state;

        public BattleRandom(uint seed) => _state = seed == 0 ? 0x9E3779B9u : seed;

        public uint NextUInt()
        {
            _state ^= _state << 13;
            _state ^= _state >> 17;
            _state ^= _state << 5;
            return _state;
        }

        // Entier dans [min, max[
        public int Range(int min, int max) => min + (int)(NextUInt() % (uint)(max - min));
    }
}

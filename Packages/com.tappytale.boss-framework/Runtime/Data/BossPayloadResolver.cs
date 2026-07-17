using System;
using System.Collections.Generic;
using TappyTale.BossFight.Core;
using TappyTale.BossFight.Feedback;
using TappyTale.BossFight.Projectiles;
using TappyTale.BossFight.Telegraphs;

namespace TappyTale.BossFight.Data
{
    public sealed class BossPayloadResolver : IBossService
    {
        private readonly BossPayloadLibrary _library;
        private readonly Dictionary<string, ProjectileDefinition> _projectiles = new(StringComparer.Ordinal);
        private readonly Dictionary<string, TelegraphDefinition> _telegraphs = new(StringComparer.Ordinal);
        private readonly Dictionary<string, AudioCueDefinition> _audio = new(StringComparer.Ordinal);
        private readonly Dictionary<string, VfxDefinition> _vfx = new(StringComparer.Ordinal);

        public BossPayloadResolver(BossPayloadLibrary library)
        {
            _library = library;
        }

        public void Initialize(BossContext context)
        {
            if (_library == null) return;
            Index(_library.Projectiles, value => value.Id, _projectiles);
            Index(_library.Telegraphs, value => value.Id, _telegraphs);
            Index(_library.AudioCues, value => value.Id, _audio);
            Index(_library.Vfx, value => value.Id, _vfx);
        }

        public bool TryGetProjectile(string id, out ProjectileDefinition value) => _projectiles.TryGetValue(id ?? string.Empty, out value);
        public bool TryGetTelegraph(string id, out TelegraphDefinition value) => _telegraphs.TryGetValue(id ?? string.Empty, out value);
        public bool TryGetAudio(string id, out AudioCueDefinition value) => _audio.TryGetValue(id ?? string.Empty, out value);
        public bool TryGetVfx(string id, out VfxDefinition value) => _vfx.TryGetValue(id ?? string.Empty, out value);

        public void Shutdown()
        {
            _projectiles.Clear();
            _telegraphs.Clear();
            _audio.Clear();
            _vfx.Clear();
        }

        private static void Index<T>(IReadOnlyList<T> source, Func<T, string> idSelector, Dictionary<string, T> target) where T : class
        {
            for (int i = 0; i < source.Count; i++)
            {
                T value = source[i];
                if (value == null) continue;
                string id = idSelector(value);
                if (!string.IsNullOrWhiteSpace(id)) target[id] = value;
            }
        }
    }
}
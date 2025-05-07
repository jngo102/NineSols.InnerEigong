using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Object = UnityEngine.Object;

namespace InnerEigong;

/// <summary>
/// Manages sprite resources for the custom sprite animator.
/// </summary>
internal static class SpriteManager {
    private static Dictionary<string, Sprite[]> AnimationToSpriteMap { get; } = [];

    public static async UniTask Initialize(string spritePrefix, int checkFirstChars = 4) {
        await LoadSprites(spritePrefix, checkFirstChars);
    }

    private static async UniTask LoadSprites(string spritePrefix, int checkFirstChars) {
        var keys = Addressables.ResourceLocators.SelectMany(locator => locator.Keys);
        foreach (var key in keys) {
            Mod.Log("Resource loc key: " + key);
        }

        var locations = await Addressables
            .LoadResourceLocationsAsync(keys, Addressables.MergeMode.Union, typeof(Sprite)).Task;
        var location = locations.FirstOrDefault(location => location.PrimaryKey == Constants.BossSceneName);
        var sprites = await Addressables.LoadAssetsAsync<Sprite>(locations,
            spr => Mod.Log("Loaded sprite: " + spr.name)).Task;
        foreach (var sprite in sprites) {
            sprites = sprites.Concat(Object.FindObjectsOfType<Sprite>(true)
                .Where(spr => spr.name.StartsWith(spritePrefix))).ToList();
            var spriteNamesWithoutPrefix =
                sprites.Select(spr => spr.name.Replace(spritePrefix, ""));
            var animsToSpriteNames = new Dictionary<string, List<string>>();
            foreach (var spriteName in spriteNamesWithoutPrefix) {
                var firstChars = spriteName.Substring(0, checkFirstChars);
                if (animsToSpriteNames.ContainsKey(firstChars))
                    animsToSpriteNames[firstChars].Add(spriteName);
                else
                    animsToSpriteNames.Add(firstChars, [spriteName]);
            }

            var spriteNameCollections = animsToSpriteNames.Values.ToList();
            foreach (var collection in spriteNameCollections) {
                var animationName = "";
                var firstSpriteName = collection[0];
                var charIndex = 0;
                while (collection.All(spriteName => spriteName.Contains(animationName))) {
                    animationName += firstSpriteName[charIndex];
                    charIndex++;
                }

                AnimationToSpriteMap.Add(animationName,
                    sprites.Where(spr => spr.name.Contains(animationName)).ToArray());
            }
        }
    }
}
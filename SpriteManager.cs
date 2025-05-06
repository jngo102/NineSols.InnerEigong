using ChartUtil;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace InnerEigong;

/// <summary>
/// Manages sprite resources for the custom sprite animator.
/// </summary>
internal static class SpriteManager {
    public static Dictionary<string, Sprite[]> AnimationToSpriteMap { get; private set; } = [];

    private static int _checkFirstChars;

    private static string _spritePrefix;

    public static void Initialize(string spritePrefix, int checkFirstChars = 4) {
        _checkFirstChars = checkFirstChars;
        _spritePrefix = spritePrefix;
        LoadSprites();
    }

    private static void LoadSprites() {
        var keys = Addressables.ResourceLocators.SelectMany(locator => locator.Keys);
        var locHandle = Addressables.LoadResourceLocationsAsync(keys, Addressables.MergeMode.Union, typeof(Sprite));
        locHandle.Completed += (handle) => {
            if (handle.Status == AsyncOperationStatus.Succeeded) {
                var locations = handle.Result;
                foreach (var loc in locations) Plugin.Instance.Log($"{loc.PrimaryKey}:\t{loc}");

                var sprHandle = Addressables.LoadAssetsAsync<Sprite>(keys, sprite => { Plugin.Instance.Log("Sprite name: " + sprite.name); },
                    Addressables.MergeMode.Union);

                sprHandle.Completed += (handle) => {
                    if (handle.Status == AsyncOperationStatus.Succeeded) {
                        var sprites = handle.Result;
                        Plugin.Instance.Log("Sprites 0: " + sprites.Count);

                        sprites.Concat(Object.FindObjectsOfType<Sprite>(true).Where(sprite => sprite.name.StartsWith(_spritePrefix)));

                        Plugin.Instance.Log("Sprites 1: " + sprites.Count);

                        var spriteNamesWithoutPrefix = sprites.Select(sprite => sprite.name.Replace(_spritePrefix, ""));
                        var animsToSpriteNames = new Dictionary<string, List<string>>();
                        foreach (var spriteName in spriteNamesWithoutPrefix) {
                            var firstChars = spriteName.Substring(0, _checkFirstChars);
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
                                sprites.Where(sprite => sprite.name.Contains(animationName)).ToArray());
                        }
                    } else {
                        Plugin.Instance.LogError("Sprite loading was not successful!");
                    }
                };
            } else {
                Plugin.Instance.LogError("Resource location loading was not successful!");
            }
        };
    }
}
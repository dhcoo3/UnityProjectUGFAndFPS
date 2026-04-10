using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using GameFramework;
using GameFramework.Resource;
using UnityEngine;
using UnityEngine.Networking;
using UnityGameFramework.Runtime;

namespace HotAssets.Scripts.Extension
{
    public static class ResourceExtension
    {
#if UNITY_EDITOR
        private static bool isSubscribeEvent = false;
#endif
        
        public static void SubscribeEvent()
        { ;
#if UNITY_EDITOR
            isSubscribeEvent = true;
#endif
        }
        
#if UNITY_EDITOR
        private static void TipsSubscribeEvent()
        {
            if (!isSubscribeEvent)
            {
                throw new Exception("Use await/async extensions must to subscribe event!");
            }
        }
#endif
        
        /// <summary>
        /// 加载资源（可等待）
        /// </summary>
        public static UniTask<T> LoadAssetAwait<T>(this ResourceComponent resourceComponent, string assetName, CancellationToken cancellationToken = default)
            where T : UnityEngine.Object
        {
#if UNITY_EDITOR
            TipsSubscribeEvent();
#endif
            UniTaskCompletionSource<T> loadAssetTcs = new UniTaskCompletionSource<T>();
            CancellationTokenRegistration registration = default;

            // 注册取消回调
            if (cancellationToken.CanBeCanceled)
            {
                registration = cancellationToken.Register(() =>
                {
                    if (loadAssetTcs != null)
                    {
                        loadAssetTcs.TrySetCanceled(cancellationToken);
                        loadAssetTcs = null;
                    }
                });
            }

            resourceComponent.LoadAsset(assetName, typeof(T), new LoadAssetCallbacks(
                (tempAssetName, asset, duration, userdata) =>
                {
                    registration.Dispose();
                    var source = loadAssetTcs;
                    loadAssetTcs = null;

                    if (source == null)
                    {
                        resourceComponent.UnloadAsset(assetName);
                        return; // 已被取消
                    }

                    T tAsset = asset as T;
                    if (tAsset != null)
                    {
                        source.TrySetResult(tAsset);
                    }
                    else
                    {
                        Log.Error($"Load asset failure load type is {asset.GetType()} but asset type is {typeof(T)}.");
                        source.TrySetException(new GameFrameworkException(
                            $"Load asset failure load type is {asset.GetType()} but asset type is {typeof(T)}."));
                    }
                },
                (tempAssetName, status, errorMessage, userdata) =>
                {
                    registration.Dispose();
                    var source = loadAssetTcs;
                    loadAssetTcs = null;

                    if (source == null) return; // 已被取消

                    Log.Error(errorMessage);
                    source.TrySetException(new GameFrameworkException(errorMessage));
                }
            ));

            return loadAssetTcs.Task;
        }

        /// <summary>
        /// 下载图片（可等待）
        /// </summary>
        /// <param name="url">图片URL地址</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>下载的Texture2D</returns>
        public static async UniTask<Texture2D> DownloadImage(string url, CancellationToken cancellationToken = default)
        {
#if UNITY_EDITOR
            TipsSubscribeEvent();
#endif
            if (string.IsNullOrEmpty(url))
            {
                throw new GameFrameworkException("Download image url is null or empty.");
            }

            using (UnityWebRequest webRequest = UnityWebRequestTexture.GetTexture(url))
            {
                CancellationTokenRegistration registration = default;

                try
                {
                    // 注册取消回调
                    if (cancellationToken.CanBeCanceled)
                    {
                        registration = cancellationToken.Register(() =>
                        {
                            webRequest?.Abort();
                        });
                    }

                    // 发送请求
                    await webRequest.SendWebRequest();

                    // 检查是否被取消
                    if (cancellationToken.IsCancellationRequested)
                    {
                        throw new OperationCanceledException(cancellationToken);
                    }

                    // 检查请求结果
                    if (webRequest.result == UnityWebRequest.Result.Success)
                    {
                        Texture2D texture = DownloadHandlerTexture.GetContent(webRequest);
                        if (texture != null)
                        {
                            return texture;
                        }
                        else
                        {
                            string errorMsg = $"Download image failed: texture is null. URL={url}";
                            Log.Error(errorMsg);
                            throw new GameFrameworkException(errorMsg);
                        }
                    }
                    else
                    {
                        string errorMsg = $"Download image failed: {webRequest.error}. URL={url}";
                        Log.Error(errorMsg);
                        throw new GameFrameworkException(errorMsg);
                    }
                }
                finally
                {
                    registration.Dispose();
                }
            }
        }
        
        /// <summary>
        /// 加载资源（可等待）
        /// </summary>
        public static UniTask<byte[]> LoadBinaryAwait(this ResourceComponent resourceComponent, string assetName, CancellationToken cancellationToken = default)
        {
#if UNITY_EDITOR
            TipsSubscribeEvent();
#endif
            UniTaskCompletionSource<byte[]> loadAssetTcs = new UniTaskCompletionSource<byte[]>();
            CancellationTokenRegistration registration = default;

            // 注册取消回调
            if (cancellationToken.CanBeCanceled)
            {
                registration = cancellationToken.Register(() =>
                {
                    if (loadAssetTcs != null)
                    {
                        loadAssetTcs.TrySetCanceled(cancellationToken);
                        loadAssetTcs = null;
                    }
                });
            }

            resourceComponent.LoadBinary(assetName,  new LoadBinaryCallbacks(
                (tempAssetName, asset, duration, userdata) =>
                {
                    registration.Dispose();
                    var source = loadAssetTcs;

                    if (source == null)
                    {
                        resourceComponent.UnloadAsset(assetName);
                        return; // 已被取消
                    }

                    if (asset != null)
                    {
                        source.TrySetResult(asset);
                    }
                    else
                    {
                        Log.Error($"Load asset failure load type is {asset.GetType()} but asset type.");
                        source.TrySetException(new GameFrameworkException(
                            $"Load asset failure load type is {asset.GetType()} but asset type is."));
                    }
                },
                (tempAssetName, status, errorMessage, userdata) =>
                {
                    registration.Dispose();
                    var source = loadAssetTcs;

                    if (source == null) return; // 已被取消

                    Log.Error(errorMessage);
                    source.TrySetException(new GameFrameworkException(errorMessage));
                }
            ));

            return loadAssetTcs.Task;
        }
    }
}

using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class BlitMaterialFeature : ScriptableRendererFeature {
    class RenderPass : ScriptableRenderPass {

        private string profilingName;
        private Material material;
        private int materialPassIndex;
        private RTHandle tempTextureHandle;

        public RenderPass(string profilingName, Material material, int passIndex) : base() {
            this.profilingName = profilingName;
            this.material = material;
            this.materialPassIndex = passIndex;
        }

        public void SetSource(RTHandle source) {
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData) {
            CommandBuffer cmd = CommandBufferPool.Get(profilingName);

            RenderTextureDescriptor cameraTextureDesc = renderingData.cameraData.cameraTargetDescriptor;
            cameraTextureDesc.depthBufferBits = 0;

            RenderingUtils.ReAllocateIfNeeded(ref tempTextureHandle, cameraTextureDesc, FilterMode.Bilinear, TextureWrapMode.Clamp, name: "_TempBlitMaterialTexture");

            Blitter.BlitCameraTexture(cmd, renderingData.cameraData.renderer.cameraColorTargetHandle, tempTextureHandle, material, materialPassIndex);
            Blitter.BlitCameraTexture(cmd, tempTextureHandle, renderingData.cameraData.renderer.cameraColorTargetHandle);

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        public void Dispose() {
            tempTextureHandle?.Release();
        }
    }

    [System.Serializable]
    public class Settings {
        public Material material;
        public int materialPassIndex = -1;
        public RenderPassEvent renderEvent = RenderPassEvent.AfterRenderingOpaques;
    }

    [SerializeField]
    private Settings settings = new Settings();

    private RenderPass renderPass;

    public Material Material {
        get => settings.material;
    }

    public override void Create() {
        this.renderPass = new RenderPass(name, settings.material, settings.materialPassIndex);
        renderPass.renderPassEvent = settings.renderEvent;
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData) {
        renderer.EnqueuePass(renderPass);
    }

    protected override void Dispose(bool disposing) {
        renderPass?.Dispose();
    }
}

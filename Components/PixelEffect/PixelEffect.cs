using Godot;

[Tool]
public partial class PixelEffect : Sprite2D {
    [Export] public SubViewport World;
    public const float PixelSize = 0.02f;
    Camera3D camera;
    Vector2 offset;
    Vector2 position;

    public override void _Ready() {
        RenderingServer.FramePostDraw += () => {
            camera.HOffset = offset.X;
            camera.VOffset = offset.Y;
            Position = position;
        };
    }

    public override void _Process(double delta) {
        CallDeferred(nameof(CameraTransform));
    }

    void CameraTransform() {
        //前处理
        camera = World.GetCamera3D();
        int renderSizeY = (int)(camera.Size / PixelSize);
        Vector2 windowSize = (Vector2)DisplayServer.WindowGetSize();
        World.Size = new Vector2I(Mathf.CeilToInt(renderSizeY * windowSize.X / windowSize.Y), renderSizeY);
        //Scale = windowSize.Y / renderSizeY * Vector2.One;
        //存储帧前数据，用来复原
        offset = new Vector2(camera.HOffset, camera.VOffset);
        position = Position;
        //计算数据
        Vector3 transformedPosition = camera.GlobalPosition * camera.GlobalTransform with {Origin = Vector3.Zero};
        Vector3 snappedPosition = transformedPosition.Snapped(new Vector3(1,1,1) * PixelSize) - transformedPosition;
        //修改数据
        camera.HOffset = snappedPosition.X;
        camera.VOffset = snappedPosition.Y;
        Position = new Vector2(snappedPosition.X, -snappedPosition.Y) / PixelSize * Scale;
    }
}
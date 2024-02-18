using Godot;
using System;
using System.Collections.Generic;

public partial class TrailRenderer : LineRenderer
{
    class TrailPiece
    {
        private static float time;

        private Vector3 lastPosition;
        private Vector3 lastSpawnPoint;
        private float remainingLifetime;
        private float aliveTime;
        private bool isMoving;
        private bool dirty = false;
        private Point firstPointOriginal;
        private LineRenderer lr;
        private TrailRenderer tr;

        public Action OnDeleteComplete;

        public TrailPiece(TrailRenderer tr)
        {
            lastPosition = tr.GlobalPosition;
            lastSpawnPoint = tr.GlobalPosition;
            remainingLifetime = tr.Lifetime;
            
            this.tr = tr;
            lr = new LineRenderer();
            tr.AddChild(lr);
        }

        public bool IsDirty()
        {
            return dirty;
        }

        public void Update(float delta)
        {
            lr.CopyValues(tr);
            time = Time.GetTicksMsec() / 1000.0f;

            if (!tr.Emitting && lr.Points.Count > 0)
                dirty = true;

            if (lr.Points.Count > 0 && remainingLifetime > 0)
            {
                aliveTime += delta;
            }

            if (lr.Points.Count == 0)
                aliveTime = 0;

            isMoving = lastPosition != lr.GlobalPosition;
            lastPosition = lr.GlobalPosition;
            remainingLifetime = lr.Points.Count > 0 ? remainingLifetime - delta : tr.Lifetime;
            remainingLifetime = Mathf.Min(remainingLifetime, tr.Lifetime);

            RemovePoints();

            if (lr.Points.Count == 0 && dirty)
            {
                OnDeleteComplete?.Invoke();
                lr.QueueFree();
            }

            AddPoints();
        }

        private void AddPoints()
        {
            if (dirty || !tr.Emitting)
                return;

            if (lr.Points.Count == 0 && isMoving)
            {
                lr.Points.Add(new Point(lr.GlobalPosition));
                lr.Points.Add(new Point(lr.GlobalPosition));
            }

            if (lastSpawnPoint.DistanceTo(lr.GlobalPosition) > tr.MinVertexDistance && lr.Points.Count > 0)
            {
                Vector3 previousPosition = lr.Points[^2].Position;
                lr.Points[^2].Position = lr.GlobalPosition;
                lr.Points.Insert(lr.Points.Count - 2, new Point(previousPosition, tr.GlobalBasis.Z));
                lastSpawnPoint = lr.GlobalPosition;
            }

            if (lr.Points.Count > 0)
                lr.Points[^1].Position = lr.GlobalPosition;
        }

        private void RemovePoints()
        {
            if (remainingLifetime > 0)
                return;

            firstPointOriginal ??= new Point(lr.Points[0].Position);

            while (lr.Points.Count > 0 && time >= lr.Points[0].Time + aliveTime)
            {
                lr.Points.RemoveAt(0);
                firstPointOriginal = lr.Points.Count > 0 ? new Point(lr.Points[0].Position) : null;
            }

            if (lr.Points.Count >= 2)
            {
                float t = Mathf.InverseLerp(firstPointOriginal.Time, lr.Points[0].Time + aliveTime, time);
                lr.Points[0].Position = firstPointOriginal.Position.Lerp(lr.Points[1].Position, t);
            }
        }
    }

    [Export] public float Lifetime = 1.0f;
    [Export] public float MinVertexDistance = 0.5f;
    [Export] public bool Emitting = true;

    private List<TrailPiece> trailPieces = new List<TrailPiece>();
    private bool emittingLastFrame;

    public override void _Ready()
    {   
        trailPieces.Add(new TrailPiece(this));
    }

    public override void _Process(double delta)
    {
        if (!emittingLastFrame && Emitting && (trailPieces.Count == 0 || trailPieces[0].IsDirty()))
        {
            trailPieces.Insert(0, new TrailPiece(this));
        }
        emittingLastFrame = Emitting;

        if (trailPieces.Count > 0)
            trailPieces[0].OnDeleteComplete = () => trailPieces.RemoveAt(trailPieces.Count - 1);

        for (int i = 0; i < trailPieces.Count; i++)
        {
            trailPieces[i].Update((float)delta);
        }
    }
}

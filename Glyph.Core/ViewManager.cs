using System.Collections.Generic;
using System.Linq;
using Glyph.Composition;
using Microsoft.Xna.Framework;

namespace Glyph.Core
{
    public class ViewManager : GlyphContainer
    {
        static private ViewManager _main;
        static public ViewManager Main
        {
            get { return _main ?? (_main = new ViewManager()); }
        }

        private SceneNode _sceneNode;
        private List<IView> _views;
        private IReadOnlyCollection<IView> _readOnlyScreens;

        public IEnumerable<IView> Views
        {
            get { return _views; }
        }

        public ViewManager()
        {
            _sceneNode = new SceneNode();
            _views = new List<IView>();
            _readOnlyScreens = _views.AsReadOnly();
        }

        public override void Initialize()
        {
            _sceneNode.Initialize();
        }

        public void Update(ElapsedTime elapsedTime)
        {
            foreach (IView screen in Views)
                screen.Update(elapsedTime);
        }

        public void RegisterScreen(IView view)
        {
            Components.Add(view);
            _views.Add(view);

            view.Initialize();
        }

        public void UnregisterScreen(IView view)
        {
            _views.Remove(view);
        }

        static public bool IsVisibleOnWindow(Vector2 position)
        {
            return Main.Views.Any(view => view.IsVisibleOnView(position));
        }

        static public bool IsVisibleOnWindow(Vector2 position, out IView view)
        {
            foreach (IView v in Main.Views)
            {
                if (!v.IsVisibleOnView(position))
                    continue;

                view = v;
                return true;
            }

            view = null;
            return false;
        }

        static public IView GetView(Point windowPoint)
        {
            return GetView(Resolution.Instance.WindowToVirtualScreen(windowPoint));
        }

        static public IView GetView(Point windowPoint, out Vector2 positionOnView)
        {
            return GetView(Resolution.Instance.WindowToVirtualScreen(windowPoint), out positionOnView);
        }

        static public IView GetView(Vector2 virtualScreenPosition)
        {
            return Main.Views.Reverse().FirstOrDefault(view => view.BoundingBox.ContainsPoint(virtualScreenPosition));
        }

        static public IView GetView(Vector2 virtualScreenPosition, out Vector2 positionOnView)
        {
            IView view = GetView(virtualScreenPosition) ?? Main.Views.First();
            positionOnView = virtualScreenPosition - view.BoundingBox.Origin;
            return view;
        }

        static public Vector2 GetPositionOnVirtualScreen(Vector2 viewPosition, IView view)
        {
            return viewPosition + view.BoundingBox.Origin;
        }
    }
}
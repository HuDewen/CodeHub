using System;
using CodeHub.Controllers;
using MonoTouch.UIKit;
using CodeFramework.Controllers;
using GitHubSharp.Models;
using CodeFramework.Elements;
using CodeHub.Filters.Models;
using CodeHub.Filters.ViewControllers;

namespace CodeHub.ViewControllers
{
    public class MyIssuesViewController : BaseListControllerDrivenViewController, IListView<IssueModel>
    {
        private readonly UISegmentedControl _viewSegment;
        private readonly UIBarButtonItem _segmentBarButton;

        public new MyIssuesController Controller
        {
            get { return (MyIssuesController)base.Controller; }
            protected set { base.Controller = value; }
        }

        public MyIssuesViewController()
        {
            Root.UnevenRows = true;
            Title = "Issues".t();
            SearchPlaceholder = "Search Issues".t();
            Controller = new MyIssuesController(this);

            _viewSegment = new UISegmentedControl(new string[] { "Open".t(), "Closed".t(), "Custom".t() });
            _viewSegment.ControlStyle = UISegmentedControlStyle.Bar;
            _segmentBarButton = new UIBarButtonItem(_viewSegment);
        }

        public void Render(ListModel<IssueModel> model)
        {
            RenderList(model, x => {
                var assigned = x.Assignee != null ? x.Assignee.Login : "unassigned";
                var kind = string.Empty;
                var commentString = x.Comments == 1 ? "1 comment".t() : x.Comments + " comments".t();
                var el = new IssueElement(x.Number.ToString(), x.Title, assigned, x.State, commentString, kind, x.UpdatedAt);
                el.Tag = x;
                el.Tapped += () => {
                    //Make sure the first responder is gone.
                    View.EndEditing(true);
                    //                    var info = new IssueInfoViewController(Controller.User, Controller.Slug, x.LocalId);
                    //                    info.Controller.ModelChanged = newModel => ChildChangedModel(newModel, x);
                    //                    NavigationController.PushViewController(info, true);
                };
                return el;
            });
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            _segmentBarButton.Width = View.Frame.Width - 10f;
            ToolbarItems = new [] { new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace), _segmentBarButton, new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace) };
        }

        protected override void SearchEnd()
        {
            base.SearchEnd();
            if (ToolbarItems != null)
                NavigationController.SetToolbarHidden(false, true);
        }

        public override void ViewWillAppear(bool animated)
        {
            if (ToolbarItems != null && !IsSearching)
                NavigationController.SetToolbarHidden(false, animated);
            base.ViewWillAppear(animated);

            //Before we select which one, make sure we detach the event handler or silly things will happen
            _viewSegment.ValueChanged -= SegmentValueChanged;

            //Select which one is currently selected
            if (Controller.Filter.Equals(MyIssuesFilterModel.CreateOpenFilter()))
                _viewSegment.SelectedSegment = 0;
            else if (Controller.Filter.Equals(MyIssuesFilterModel.CreateClosedFilter()))
                _viewSegment.SelectedSegment = 1;
            else
                _viewSegment.SelectedSegment = 2;

            _viewSegment.ValueChanged += SegmentValueChanged;
        }

        void SegmentValueChanged (object sender, EventArgs e)
        {
            if (_viewSegment.SelectedSegment == 0)
            {
                Controller.ApplyFilter(MyIssuesFilterModel.CreateOpenFilter(), true, false);
                UpdateAndRender();
            }
            else if (_viewSegment.SelectedSegment == 1)
            {
                Controller.ApplyFilter(MyIssuesFilterModel.CreateClosedFilter(), true, false);
                UpdateAndRender();
            }
            else if (_viewSegment.SelectedSegment == 2)
            {
                var filter = new  MyIssuesFilterViewController(Controller);
                var nav = new UINavigationController(filter);
                PresentViewController(nav, true, null);
            }
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);
            if (ToolbarItems != null)
                NavigationController.SetToolbarHidden(true, animated);
        }
    }
}


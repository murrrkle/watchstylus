
using Android.App;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Astral.Droid.UI;

namespace Astral.Droid.Fragments
{
    [Register("Astral.Droid.Fragment.AirbrushFragment")]
    public class AirbrushFragment : Fragment
    {
        public delegate void AirbrushEventHandler(object sender, float airflow);
        public event AirbrushEventHandler AirflowChanged;

        public AirbrushImageView aiv;
        public AirbrushFragment()
        {
        }

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your fragment here
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            // Use this to return your custom view for this Fragment
             return inflater.Inflate(Resource.Layout.AirbrushUI, container, false);

            //return base.OnCreateView(inflater, container, savedInstanceState);

        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);

            aiv = view.FindViewById<AirbrushImageView>(Resource.Id.AirbrushTrigger);
            aiv.AirflowChanged += Aiv_AirflowChanged;
        }

        private void Aiv_AirflowChanged(object sender, float airflow)
        {
            AirflowChanged?.Invoke(this, airflow);
        }
    }
}
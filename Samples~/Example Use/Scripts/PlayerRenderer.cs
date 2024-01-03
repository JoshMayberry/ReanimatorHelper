using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace jmayberry.ReanimatorHelper.Samples.ExampleUse {
	public enum SlimeAudioEvents {
		OnMove,
    }

    public enum SlimeListenEvents {
        OnTwistInFinished,
        OnTwistOutFinished,
        OnHurtFinished,
    }

    public enum SlimeUpdateEvents {
        OnTwistIn,
        OnHurt,
        OnActionStarting,
    }

    public enum SlimeFixedUpdateEvents {
    }

    public class PlayerRenderer : BaseRenderer<SlimeAudioEvents, SlimeListenEvents, SlimeUpdateEvents, SlimeFixedUpdateEvents> {

	}
}

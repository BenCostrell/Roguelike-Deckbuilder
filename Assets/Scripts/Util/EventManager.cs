using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class GameEvent {}

// EVENTS
public class OptionChosen : GameEvent
{
    public int optionChosen;

    public OptionChosen(int optionChosen_)
    {
        optionChosen = optionChosen_;
    }
}

public class AcquisitionComplete : GameEvent { }

public class MovementInitiated : GameEvent { }

public class ButtonPressed : GameEvent {
    public string button;
    public int playerNum;
    public ButtonPressed(string _button, int _playerNum)
    {
        button = _button;
        playerNum = _playerNum;
    }
}

public class Reset : GameEvent { }

public class TileSelected : GameEvent
{
    public Tile tile;
    public TileSelected(Tile tile_)
    {
        tile = tile_;
    }
}

public class TileHovered : GameEvent {
    public Tile tile;
    public TileHovered(Tile tile_)
    {
        tile = tile_;
    }
}


public class CardSelected : GameEvent
{
    public Card card;
    public CardSelected(Card card_)
    {
        card = card_;
    }
}


public class EventManager {

	public delegate void EventDelegate<T>(T e) where T: GameEvent;
	private delegate void EventDelegate(GameEvent e);

	private Dictionary <System.Type, EventDelegate> delegates = new Dictionary<System.Type, EventDelegate>();
	private Dictionary<System.Delegate, EventDelegate> delegateLookup = new Dictionary<System.Delegate, EventDelegate>();
	private List<GameEvent> queuedEvents = new List<GameEvent> ();
	private object queueLock = new object();

	public void Register<T> (EventDelegate<T> del) where T: GameEvent {
		if (delegateLookup.ContainsKey (del)) {
			return;
		}

		EventDelegate internalDelegate = (e) => del ((T)e);
		delegateLookup [del] = internalDelegate;

		EventDelegate tempDel;
		if (delegates.TryGetValue (typeof(T), out tempDel)) {
			delegates [typeof(T)] = tempDel + internalDelegate;
		} else {
			delegates [typeof(T)] = internalDelegate;
		}
	}

	public void Unregister<T> (EventDelegate<T> del) where T: GameEvent {
		EventDelegate internalDelegate;
		if (delegateLookup.TryGetValue (del, out internalDelegate)) {
			EventDelegate tempDel;
			if (delegates.TryGetValue (typeof(T), out tempDel)) {
				tempDel -= internalDelegate;
				if (tempDel == null) {
					delegates.Remove (typeof(T));
				} else {
					delegates [typeof(T)] = tempDel;
				}
			}
			delegateLookup.Remove (del);
		}
	}

	public void Clear(){
		lock (queueLock) {
			if (delegates != null) {
				delegates.Clear ();
			}
			if (delegateLookup != null) {
				delegateLookup.Clear ();
			}
			if (queuedEvents != null) {
				queuedEvents.Clear ();
			}
		}
	}

	public void Fire(GameEvent e){
		EventDelegate del;
		if (delegates.TryGetValue (e.GetType (), out del)) {
			del.Invoke (e);
		}
	}

	public void ProcessQueuedEvents(){
		List<GameEvent> events;
		lock (queueLock) {
			if (queuedEvents.Count > 0) {
				events = new List<GameEvent> (queuedEvents);
				queuedEvents.Clear ();
			} else {
				return;
			}
		}

		foreach (GameEvent e in events) {
			Fire (e);
		}
	}

	public void Queue(GameEvent e){
		lock (queueLock) {
			queuedEvents.Add (e);
		}
	}

}

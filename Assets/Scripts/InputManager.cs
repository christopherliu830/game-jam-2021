/*
using UnityEngine;

public struct Inputs {
  public float dx;
  public float dy;
  public bool jump;
  public bool mouse0;
  public bool mouse1;

  public void Clear() {
    dx = dy = 0;
    jump = mouse0 = mouse1 = false;
  }
}


public class InputManager : MonoBehaviour {
  public static InputManager Instance => _instance;
  private static InputManager _instance;
  private Inputs _current;
  private MonoBehaviour _controller;

  public delegate void InputEventHandler(KeyCode key);
  public InputEventHandler DownEvent;
  public InputEventHandler UpEvent;
  
  void Awake() {
    if (_instance == null) _instance = this;
    else Debug.LogError("Warning: Multiple " + this);
  }

  private void Update() {
    if (Input.GetKeyDown(KeyCode.Space)) {
      _current.jump = true;
      DownEvent?.Invoke(KeyCode.Space);
    }
    if (Input.GetKeyUp(KeyCode.Space)) {
      _current.jump = false;
      UpEvent?.Invoke(KeyCode.Space);
    }
    if (Input.GetMouseButtonDown(0)) {
      _current.mouse0 = true;
      DownEvent?.Invoke(KeyCode.Mouse0);
    }
    if (Input.GetMouseButtonDown(1)) {
      _current.mouse1 = true;
      DownEvent?.Invoke(KeyCode.Mouse1);
    }
    if (Input.GetMouseButtonUp(0)) {
      _current.mouse0 = false;
      UpEvent?.Invoke(KeyCode.Mouse0);
    }
    if (Input.GetMouseButtonUp(1)) {
      _current.mouse1 = false;
      UpEvent?.Invoke(KeyCode.Mouse1);
    }
    _current.dx = Input.GetAxisRaw("Horizontal");
    _current.dy = Input.GetAxisRaw("Horizontal");
  }

  // Change the subscriber of the event
  void TakeInput(IInputHandler asker) {
    if (_controller == asker) return;
    DownEvent -= _controller.OnKeyDown;
    UpEvent -= _controller.OnKeyUp;
    _controller = asker;
    DownEvent += asker.OnKeyDown;
    UpEvent += asker.OnKeyUp;
  }

  Inputs GetInput(IInputHandler asker) {
    if (_controller == asker) return new Inputs();
    else return _current;
  }

}

public interface IInputHandler {
  void OnKeyDown(KeyCode key);
  void OnKeyUp(KeyCode key);
}
*/
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;

namespace MJH.UIComponents
{
    enum Direction { MaxLeft, Left, Center, Right, MaxRight }

    public delegate void Swiped(bool rightSwipe);

    [RequireComponent(typeof(RectTransform))]
    public class SwipeCard : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler
    {
        
        [Range(0, 90), Tooltip("Max rotation of the card in degrees")]
        public int rotation = 20;

        [Tooltip("Maximum drag left")]
        public int minDrag = -300;

        [Tooltip("Maximum drag right")]
        public int maxDrag = 300;

        [Range(0, 3), Tooltip("Time to snap back an uncompleted swipe")]
        public float snapBackSpeed = 1;

        [Tooltip("Image component to fade in when close to the left")]
        public Image leftIcon;

        [Tooltip("Image component to fade in when close to the right")]
        public Image rightIcon;

        [Tooltip("Images that will fade out when image is dragged")]
        public Image[] fadeOut;

        public event Swiped onSwiped;

        private Direction _dir;
        private bool _snapping;
        private bool _dragging;
        private RectTransform _rectTrans;
        private Vector3 _offset;
        private Vector3 _startPos;
        private Limit minOrMax;

        private delegate int Limit(Direction d);

        public void Start()
        {
            _rectTrans = GetComponent<RectTransform>();
            _startPos = _rectTrans.localPosition;

            if (leftIcon)
                leftIcon.color = new Color(1,1,1,0);

            if (rightIcon)
                rightIcon.color = new Color(1,1,1,0);

            minOrMax = d => {
                if (d == Direction.Right || d == Direction.MaxRight )
                    return maxDrag;
                else
                    return minDrag;
            };
        }

        public void OnBeginDrag(PointerEventData aEventData)
        {
            _snapping = false;
            _dragging = true;
            _offset = Input.mousePosition - _rectTrans.position;
        }

        public void OnEndDrag(PointerEventData aEventData)
        {
            _dragging = false;
            if(_rectTrans.position != _startPos)
            {
                _snapping = true;

                if (_rectTrans.localPosition.x == minOrMax(GetDirection()))
                {
                    if(onSwiped != null)
                        onSwiped(_rectTrans.localPosition.x == maxDrag ? true : false);
                }

                StartCoroutine(SnapBack());
            }
        }

        public void OnDrag(PointerEventData aEventData)
        {
            if (!_dragging)
                return;

            _rectTrans.position = new Vector3(Input.mousePosition.x - _offset.x, Input.mousePosition.y - _offset.y, _rectTrans.position.z);

            _dir = GetDirection();

            switch (_dir)
            {
                case Direction.MaxRight:
                    _rectTrans.localPosition = new Vector3(maxDrag, _rectTrans.localPosition.y, _rectTrans.localPosition.z);
                    break;

                case Direction.MaxLeft:
                    _rectTrans.localPosition = new Vector3(minDrag, _rectTrans.localPosition.y, _rectTrans.localPosition.z);
                    break;
            }

            float lerp = GetLerp(_dir);

            RotateLrp(_dir, lerp);

            AlphaLrp(_dir, lerp);
        }

        private IEnumerator SnapBack()
        {
            float sLerp = 0;

            while (_snapping)
            {
                _dir = GetDirection();

                float rLerp = GetLerp(_dir);

                sLerp += 0.02f * snapBackSpeed;

                _rectTrans.localPosition = Vector3.Lerp(_rectTrans.localPosition, _startPos, sLerp);

                RotateLrp(_dir, rLerp);
                AlphaLrp(_dir, rLerp);

                if (rLerp <= 0.01)
                    _snapping = false;

                yield return new WaitForEndOfFrame();
            }

        }

        private void RotateLrp(Direction dir, float lerp)
        {
            int rot = rotation;

            if (dir == Direction.Right || dir == Direction.MaxRight)
                rot = -rot;

            _rectTrans.rotation = Quaternion.Euler(Vector3.Lerp(new Vector3(0, 0, 0), new Vector3(0, 0, rot), Clamp(lerp, 0, 1)));
        }

        private void AlphaLrp(Direction dir, float lerp)
        {
            if (fadeOut != null)
            {
                for (int i = 0; i < fadeOut.Length; i++)
                {
                    if (fadeOut[i])
                        fadeOut[i].color = new Color(1, 1, 1, Clamp(1 - lerp, 0.3f, 1));
                }
            }

            switch (_dir)
            {
                case Direction.Left:
                    if (leftIcon)
                        leftIcon.color = new Color(1, 1, 1, lerp);
                    break;

                case Direction.Right:
                    if (rightIcon)
                        rightIcon.color = new Color(1, 1, 1, lerp);
                    break;
            }
        }

        private float GetLerp(Direction dir)
        {
            return _rectTrans.localPosition.x / minOrMax(dir);
        }

        private float Clamp(float value, float min, float max)
        {
            return (value < min) ? min : (value > max) ? max : value;
        }

        private Direction GetDirection()
        {
            float pos = _rectTrans.localPosition.x;
            Direction d;
            if(pos < _startPos.x)
            {
                d = pos <= minDrag ? Direction.MaxLeft : Direction.Left;
            }else if(pos > _startPos.x)
            {
                d = pos >= maxDrag ? Direction.MaxRight : Direction.Right;
            }
            else
            {
                d = Direction.Center;
            }

            return d;
        }

    }
}


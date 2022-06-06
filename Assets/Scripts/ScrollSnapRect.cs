using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Image))]
[RequireComponent(typeof(Mask))]
[RequireComponent(typeof(ScrollRect))]
public class ScrollSnapRect : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler {

    [Tooltip("Set starting page index - starting from 0")]
    public int startingPage = 0;
    [Tooltip("Threshold time for fast swipe in seconds")]
    public float fastSwipeThresholdTime = 0.3f;
    [Tooltip("Threshold time for fast swipe in (unscaled) pixels")]
    public int fastSwipeThresholdDistance = 100;
    [Tooltip("How fast will page lerp to target position")]
    public float decelerationRate = 10f;
    [Tooltip("Button to go to the previous page (optional)")]
    public GameObject prevButton;
    [Tooltip("Button to go to the next page (optional)")]
    public GameObject nextButton;
    [Tooltip("Sprite for unselected page (optional)")]
    public Sprite unselectedPage;
    [Tooltip("Sprite for selected page (optional)")]
    public Sprite selectedPage;
    [Tooltip("Container with page images (optional)")]
    public Transform pageSelectionIcons;

    public Color[] dot_colors;

    private int _fastSwipeThresholdMaxLimit;

    private ScrollRect _scrollRectComponent;
    private RectTransform _scrollRectRect;
    private RectTransform _container;

    private bool _horizontal;
    
    private int _pageCount;
    private int _currentPage;

    private bool _lerp;
    private Vector2 _lerpTo;

    private List<Vector2> _pagePositions = new List<Vector2>();

    private bool _dragging;
    private float _timeStamp;
    private Vector2 _startPosition;

    private bool _showPageSelection;
    private int _previousPageSelectionIndex;
    private List<Image> _pageSelectionImages;



    //------------------------------------------------------------------------
    void Start() {


        _scrollRectComponent = GetComponent<ScrollRect>();
        _scrollRectRect = GetComponent<RectTransform>();
        _container = _scrollRectComponent.content;
        _pageCount = _container.childCount;

        if (_scrollRectComponent.horizontal && !_scrollRectComponent.vertical) {
            _horizontal = true;
        } else if (!_scrollRectComponent.horizontal && _scrollRectComponent.vertical) {
            _horizontal = false;
        } else {
            _horizontal = true;
        }


        _lerp = false;

        // init
        SetPagePositions();
        SetPage(startingPage);
        InitPageSelection();
        SetPageSelection(startingPage);

        // prev and next buttons
        if (nextButton)
            nextButton.GetComponent<Button>().onClick.AddListener(() => { NextScreen(); });

        if (prevButton)
            prevButton.GetComponent<Button>().onClick.AddListener(() => { PreviousScreen(); });
	}

    public void GoBack2()
    {
        PreviousScreen();
    }

    public void GoNext()
    {
        NextScreen();
    }
    void Update() {


        if (Signing.reg_fine == true)
        {
            NextScreen();
            Signing.reg_fine = false;
        }

        if (_lerp) {
            float decelerate = Mathf.Min(decelerationRate * Time.deltaTime, 1f);
            _container.anchoredPosition = Vector2.Lerp(_container.anchoredPosition, _lerpTo, decelerate);
            if (Vector2.SqrMagnitude(_container.anchoredPosition - _lerpTo) < 0.25f) {
                _container.anchoredPosition = _lerpTo;
                _lerp = false;
                _scrollRectComponent.velocity = Vector2.zero;
            }

            if (_showPageSelection) {
                SetPageSelection(GetNearestPage());
            }
        }
    }

    //------------------------------------------------------------------------
    private void SetPagePositions() {
        int width = 0;
        int height = 0;
        int offsetX = 0;
        int offsetY = 0;
        int containerWidth = 0;
        int containerHeight = 0;

        if (_horizontal) {
            width = (int)_scrollRectRect.rect.width;
            offsetX = width / 2;
            containerWidth = width * _pageCount;
            _fastSwipeThresholdMaxLimit = width;
        } else {
            height = (int)_scrollRectRect.rect.height;
            offsetY = height / 2;
            containerHeight = height * _pageCount;
            _fastSwipeThresholdMaxLimit = height;
        }

        Vector2 newSize = new Vector2(containerWidth, containerHeight);
        _container.sizeDelta = newSize;
        Vector2 newPosition = new Vector2(containerWidth / 2, containerHeight / 2);
        _container.anchoredPosition = newPosition;

        _pagePositions.Clear();

        for (int i = 0; i < _pageCount; i++) {
            RectTransform child = _container.GetChild(i).GetComponent<RectTransform>();
            Vector2 childPosition;
            if (_horizontal) {
                childPosition = new Vector2(i * width - containerWidth / 2 + offsetX, 0f);
            } else {
                childPosition = new Vector2(0f, -(i * height - containerHeight / 2 + offsetY));
            }
            child.anchoredPosition = childPosition;
            _pagePositions.Add(-childPosition);
        }
    }

    private void SetPage(int aPageIndex) {
        aPageIndex = Mathf.Clamp(aPageIndex, 0, _pageCount - 1);
        _container.anchoredPosition = _pagePositions[aPageIndex];
        _currentPage = aPageIndex;
    }

    private void LerpToPage(int aPageIndex) {
        aPageIndex = Mathf.Clamp(aPageIndex, 0, _pageCount - 1);
        _lerpTo = _pagePositions[aPageIndex];
        _lerp = true;
        _currentPage = aPageIndex;
    }

    private void InitPageSelection() {
        _showPageSelection = unselectedPage != null && selectedPage != null;
        if (_showPageSelection) {
            if (pageSelectionIcons == null || pageSelectionIcons.childCount != _pageCount) {
                _showPageSelection = false;
            } else {
                _previousPageSelectionIndex = -1;
                _pageSelectionImages = new List<Image>();

                for (int i = 0; i < pageSelectionIcons.childCount; i++) {
                    Image image = pageSelectionIcons.GetChild(i).GetComponent<Image>();
                    if (image == null) {
                    }
                    _pageSelectionImages.Add(image);
                }
            }
        }
    }

    private void SetPageSelection(int aPageIndex) {
        if (_previousPageSelectionIndex == aPageIndex) {
            return;
        }
        
        if (_previousPageSelectionIndex >= 0) {
            _pageSelectionImages[_previousPageSelectionIndex].color = dot_colors[1];
        }

        _pageSelectionImages[aPageIndex].color = dot_colors[0];

        _previousPageSelectionIndex = aPageIndex;
    }

    private void NextScreen() {
        Debug.Log(_currentPage);
        LerpToPage(_currentPage + 1);
    }

    private void PreviousScreen() {
        LerpToPage(_currentPage - 1);
    }

    private int GetNearestPage() {
        Vector2 currentPosition = _container.anchoredPosition;

        float distance = float.MaxValue;
        int nearestPage = _currentPage;

        for (int i = 0; i < _pagePositions.Count; i++) {
            float testDist = Vector2.SqrMagnitude(currentPosition - _pagePositions[i]);
            if (testDist < distance) {
                distance = testDist;
                nearestPage = i;
            }
        }

        return nearestPage;
    }

    public void OnBeginDrag(PointerEventData aEventData) {
        _lerp = false;
        _dragging = false;
    }

    public void OnEndDrag(PointerEventData aEventData) {
        float difference;
        if (_horizontal) {
            difference = _startPosition.x - _container.anchoredPosition.x;
        } else {
            difference = - (_startPosition.y - _container.anchoredPosition.y);
        }

        if (Time.unscaledTime - _timeStamp < fastSwipeThresholdTime &&
            Mathf.Abs(difference) > fastSwipeThresholdDistance &&
            Mathf.Abs(difference) < _fastSwipeThresholdMaxLimit) {
            if (difference > 0) {
                NextScreen();
            } else {
                PreviousScreen();
            }
        } else {
            LerpToPage(GetNearestPage());
        }

        _dragging = false;
    }

    public void OnDrag(PointerEventData aEventData) {
        if (!_dragging) {
            _dragging = true;
            _timeStamp = Time.unscaledTime;
            _startPosition = _container.anchoredPosition;
        } else {
            if (_showPageSelection) {
                SetPageSelection(GetNearestPage());
            }
        }
    }
}

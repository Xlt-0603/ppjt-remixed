using System;
using System.Collections.Generic;
using UnityEngine;

namespace PPCorps
{
    [DefaultExecutionOrder(-100)]
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("节拍设置")]
        [SerializeField] private float _bpm = 60f;
        [SerializeField] private bool _startPaused;

        [Header("引用")]
        [SerializeField] private Commander _commander;

        [Header("胜负判定 - 双方塔")]
        [SerializeField] private UnitBase _playerTower;
        [SerializeField] private UnitBase _enemyTower;

        public int Bar { get; private set; }
        public int Beat { get; private set; }
        public float BPM
        {
            get => _bpm;
            set => _bpm = Mathf.Clamp(value, 1f, 300f);
        }
        public GameState State { get; private set; }
        public bool IsPaused { get; private set; }

        public event Action<int, int> OnBeat;
        public event Action<GameState> OnStateChanged;

        private List<UnitBase> _allUnits = new List<UnitBase>();
        private float _beatTimer;
        private bool _playerTowerDead;
        private bool _enemyTowerDead;

        private void Awake()
        {
            Instance = this;
            Bar = 1;
            Beat = 1;
            IsPaused = _startPaused;
            SetState(GameState.Deploy);
        }

        private void Start()
        {
            if (_playerTower != null)
                _playerTower.OnUnitDeath += OnPlayerTowerDeath;
            if (_enemyTower != null)
                _enemyTower.OnUnitDeath += OnEnemyTowerDeath;
        }

        private void OnPlayerTowerDeath(UnitBase _) => _playerTowerDead = true;
        private void OnEnemyTowerDeath(UnitBase _) => _enemyTowerDead = true;

        private void Update()
        {
            if (State != GameState.Battle) return;
            if (IsPaused) return;

            _beatTimer += Time.deltaTime;
            float beatInterval = 60f / (_bpm * 8f);

            while (_beatTimer >= beatInterval)
            {
                _beatTimer -= beatInterval;
                AdvanceOneBeat();
            }
        }

        private void AdvanceOneBeat()
        {
            Beat++;
            if (Beat > 8)
            {
                Beat = 1;
                Bar++;
            }

            OnBeat?.Invoke(Bar, Beat);

            var unitsThisBeat = new List<UnitBase>(_allUnits);
            unitsThisBeat.Sort((a, b) =>
            {
                if (a == null || b == null) return 0;
                if (a.IsEnemy != b.IsEnemy)
                    return a.IsEnemy.CompareTo(b.IsEnemy);
                return a.IsEnemy
                    ? a.GridPos.col.CompareTo(b.GridPos.col)
                    : b.GridPos.col.CompareTo(a.GridPos.col);
            });
            foreach (var unit in unitsThisBeat)
            {
                if (unit != null) unit.OnBeat(Bar, Beat);
            }

            CheckGameOver();
        }

        private bool _forceWin;

        public void ForceWin()
        {
            _forceWin = true;
        }

        private void CheckGameOver()
        {
            if (_forceWin)
            {
                SetState(GameState.Win);
                return;
            }
            if (_playerTowerDead)
                SetState(GameState.Lose);
            else if (_enemyTowerDead)
                SetState(GameState.Win);
        }

        public void SetState(GameState newState)
        {
            State = newState;
            OnStateChanged?.Invoke(newState);
        }

        public void RegisterUnit(UnitBase unit)
        {
            if (!_allUnits.Contains(unit))
                _allUnits.Add(unit);
        }

        public void UnregisterUnit(UnitBase unit)
        {
            _allUnits.Remove(unit);
        }

        public List<UnitBase> GetAllUnits() => _allUnits;

        public Commander GetCommander() => _commander;

        private void OnDestroy()
        {
            if (_playerTower != null)
                _playerTower.OnUnitDeath -= OnPlayerTowerDeath;
            if (_enemyTower != null)
                _enemyTower.OnUnitDeath -= OnEnemyTowerDeath;
        }

        [ContextMenu("步进一拍")]
        public void StepBeat()
        {
            if (State == GameState.Battle)
            {
                IsPaused = true;
                AdvanceOneBeat();
            }
        }

        [ContextMenu("暂停/继续")]
        public void TogglePause()
        {
            IsPaused = !IsPaused;
        }

        public void SetBPM(float bpm)
        {
            BPM = bpm;
        }

        [ContextMenu("开始战斗")]
        public void StartBattle()
        {
            if (State == GameState.Deploy)
            {
                _beatTimer = 0f;
                IsPaused = false;
                SetState(GameState.Battle);
            }
        }
    }
}

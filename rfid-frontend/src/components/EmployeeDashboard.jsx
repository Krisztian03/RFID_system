import React, { useState, useEffect, useCallback } from 'react';
import { useNavigate } from 'react-router-dom';
import { adminAPI } from '../services/api';
import './Dashboard.css';

function EmployeeDashboard() {
  const [summary, setSummary] = useState([]);
  const [loading, setLoading] = useState(false);
  const [selectedDate, setSelectedDate] = useState(new Date(2025, 8, 1)); // Szeptember 2025
  const navigate = useNavigate();

  const user = JSON.parse(localStorage.getItem('user') || '{}');

  const fetchMySummary = useCallback(async () => {
    setLoading(true);
    try {
      // A kiválasztott hónap első és utolsó napja
      const year = selectedDate.getFullYear();
      const month = selectedDate.getMonth();
      
      const from = new Date(year, month, 1, 0, 0, 0);
      const to = new Date(year, month + 1, 0, 23, 59, 59); // Utolsó nap a hónapban

      console.log('Lekérdezés:', from.toISOString(), 'to', to.toISOString());

      const response = await adminAPI.getSummary(
        user.employeeId,
        from.toISOString(),
        to.toISOString()
      );
      
      console.log('Summary response:', response.data);
      setSummary(response.data || []);
    } catch (error) {
      console.error('Hiba a statisztikák lekérésénél:', error);
    } finally {
      setLoading(false);
    }
  }, [user.employeeId, selectedDate]);

  useEffect(() => {
    fetchMySummary();
  }, [fetchMySummary]);

  const handleLogout = () => {
    localStorage.removeItem('token');
    localStorage.removeItem('user');
    navigate('/login');
  };

  const previousMonth = () => {
    setSelectedDate(new Date(selectedDate.getFullYear(), selectedDate.getMonth() - 1, 1));
  };

  const nextMonth = () => {
    setSelectedDate(new Date(selectedDate.getFullYear(), selectedDate.getMonth() + 1, 1));
  };

  const goToCurrentMonth = () => {
    setSelectedDate(new Date(2025, 8, 1)); // Szeptember 2025 (ahol vannak adatok)
  };

  const totalHours = summary.reduce((acc, day) => acc + day.hours, 0);
  const totalAmount = summary.reduce((acc, day) => acc + day.amount, 0);

  const monthYearText = selectedDate.toLocaleDateString('hu-HU', { 
    year: 'numeric', 
    month: 'long' 
  });

  return (
    <div className="dashboard employee-dashboard">
      <header className="dashboard-header">
        <h1>👤 Saját Munkaóráim</h1>
        <div className="user-info">
          <span>Üdv, {user.employeeName || user.email}!</span>
          <button onClick={handleLogout} className="btn-secondary">Kijelentkezés</button>
        </div>
      </header>

      <div className="dashboard-content single-column">
        <div className="summary-overview">
          <div className="month-selector">
            <button onClick={previousMonth} className="month-nav-btn">◀</button>
            <h2 className="month-title">📅 {monthYearText}</h2>
            <button onClick={nextMonth} className="month-nav-btn">▶</button>
          </div>
          <button onClick={goToCurrentMonth} className="btn-today">
            Mai hónap
          </button>
          
          {loading ? (
            <div className="loading">Betöltés...</div>
          ) : (
            <>
              <div className="summary-cards">
                <div className="summary-card big">
                  <div className="summary-value">{totalHours.toFixed(1)} óra</div>
                  <div className="summary-label">Összes munkaidő</div>
                </div>
                <div className="summary-card big">
                  <div className="summary-value">{totalAmount.toLocaleString()} Ft</div>
                  <div className="summary-label">Havi fizetés</div>
                </div>
                <div className="summary-card big">
                  <div className="summary-value">{summary.length} nap</div>
                  <div className="summary-label">Munkában töltött napok</div>
                </div>
              </div>
            </>
          )}
        </div>

        {!loading && summary.length > 0 ? (
          <div className="daily-breakdown">
            <h3>Napi bontás</h3>
            <table className="summary-table">
              <thead>
                <tr>
                  <th>Dátum</th>
                  <th>Munkaórák</th>
                  <th>Fizetés</th>
                </tr>
              </thead>
              <tbody>
                {summary.map((day, idx) => (
                  <tr key={idx}>
                    <td>
                      {new Date(day.day).toLocaleDateString('hu-HU', {
                        weekday: 'short',
                        year: 'numeric',
                        month: 'short',
                        day: 'numeric'
                      })}
                    </td>
                    <td>{day.hours.toFixed(2)} óra</td>
                    <td>{day.amount.toLocaleString()} Ft</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        ) : !loading && (
          <div className="no-data">
            <p>📭 Nincs rögzített munkaidő ebben a hónapban.</p>
            <p>Az RFID kártyáddal való belépés/kilépés automatikusan rögzítésre kerül!</p>
          </div>
        )}
      </div>
    </div>
  );
}

export default EmployeeDashboard;
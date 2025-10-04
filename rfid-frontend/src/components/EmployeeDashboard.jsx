import React, { useState, useEffect, useCallback } from 'react';
import { useNavigate } from 'react-router-dom';
import { adminAPI } from '../services/api';
import './Dashboard.css';

function EmployeeDashboard() {
  const [summary, setSummary] = useState([]);
  const [loading, setLoading] = useState(true);
  const navigate = useNavigate();

  const user = JSON.parse(localStorage.getItem('user') || '{}');

  const fetchMySummary = useCallback(async () => {
    try {
      const now = new Date();
      const from = new Date(now.getFullYear(), now.getMonth(), 1);
      const to = new Date();

      const response = await adminAPI.getSummary(
        user.employeeId,
        from.toISOString(),
        to.toISOString()
      );
      
      setSummary(response.data);
    } catch (error) {
      console.error('Hiba a statisztikák lekérésénél:', error);
    } finally {
      setLoading(false);
    }
  }, [user.employeeId]);

  useEffect(() => {
    fetchMySummary();
  }, [fetchMySummary]);

  const handleLogout = () => {
    localStorage.removeItem('token');
    localStorage.removeItem('user');
    navigate('/login');
  };

  if (loading) return <div className="loading">Betöltés...</div>;

  const totalHours = summary.reduce((acc, day) => acc + day.hours, 0);
  const totalAmount = summary.reduce((acc, day) => acc + day.amount, 0);

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
          <h2>📅 {new Date().toLocaleDateString('hu-HU', { year: 'numeric', month: 'long' })}</h2>
          
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
        </div>

        {summary.length > 0 ? (
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
        ) : (
          <div className="no-data">
            <p>📭 Még nincs rögzített munkaidő ebben a hónapban.</p>
            <p>Az RFID kártyáddal való belépés/kilépés automatikusan rögzítésre kerül!</p>
          </div>
        )}
      </div>
    </div>
  );
}

export default EmployeeDashboard;
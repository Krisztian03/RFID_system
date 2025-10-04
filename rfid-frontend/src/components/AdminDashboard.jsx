import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { employeeAPI, adminAPI, authAPI } from '../services/api';
import './Dashboard.css';

function AdminDashboard() {
  const [employees, setEmployees] = useState([]);
  const [selectedEmployee, setSelectedEmployee] = useState(null);
  const [summary, setSummary] = useState(null);
  const [showAddModal, setShowAddModal] = useState(false);
  const [loading, setLoading] = useState(true);
  const navigate = useNavigate();

  const user = JSON.parse(localStorage.getItem('user') || '{}');

  useEffect(() => {
    fetchEmployees();
  }, []);

  const fetchEmployees = async () => {
    try {
      const response = await employeeAPI.getAll();
      setEmployees(response.data);
    } catch (error) {
      console.error('Hiba a dolgozók lekérésénél:', error);
    } finally {
      setLoading(false);
    }
  };

  const fetchSummary = async (employeeId) => {
    try {
      const now = new Date();
      const from = new Date(now.getFullYear(), now.getMonth(), 1);
      const to = new Date();

      const response = await adminAPI.getSummary(employeeId, from.toISOString(), to.toISOString());
      setSummary(response.data);
    } catch (error) {
      console.error('Hiba a statisztikák lekérésénél:', error);
    }
  };

  const handleSelectEmployee = (employee) => {
    setSelectedEmployee(employee);
    fetchSummary(employee.id);
  };

  const handleLogout = () => {
    localStorage.removeItem('token');
    localStorage.removeItem('user');
    navigate('/login');
  };

  const handleDeleteEmployee = async (id) => {
    if (!window.confirm('Biztosan törölni szeretnéd ezt a dolgozót?')) return;

    try {
      await employeeAPI.delete(id);
      fetchEmployees();
      setSelectedEmployee(null);
      setSummary(null);
      alert('Dolgozó törölve!');
    } catch (error) {
      alert('Hiba a törlés során!');
    }
  };

  if (loading) return <div className="loading">Betöltés...</div>;

  return (
    <div className="dashboard">
      <header className="dashboard-header">
        <h1>👔 Admin Dashboard</h1>
        <div className="user-info">
          <span>Üdv, {user.email}!</span>
          <button onClick={handleLogout} className="btn-secondary">Kijelentkezés</button>
        </div>
      </header>

      <div className="dashboard-content">
        <div className="sidebar">
          <div className="sidebar-header">
            <h2>Dolgozók</h2>
            <button onClick={() => setShowAddModal(true)} className="btn-primary">+ Új dolgozó</button>
          </div>

          <div className="employee-list">
            {employees.map((emp) => (
              <div
                key={emp.id}
                className={`employee-card ${selectedEmployee?.id === emp.id ? 'active' : ''}`}
                onClick={() => handleSelectEmployee(emp)}
              >
                <div className="employee-name">{emp.name}</div>
                <div className="employee-role">{emp.role}</div>
                <div className="employee-rfid">🔑 {emp.rfidUid}</div>
              </div>
            ))}
          </div>
        </div>

        <div className="main-content">
          {selectedEmployee ? (
            <>
              <div className="employee-details">
                <h2>{selectedEmployee.name}</h2>
                <p>Szerepkör: <strong>{selectedEmployee.role}</strong></p>
                <p>RFID: <strong>{selectedEmployee.rfidUid}</strong></p>
                <p>Órabér: <strong>{selectedEmployee.hourlyRate} Ft/óra</strong></p>
                
                <div className="actions">
                  <button className="btn-danger" onClick={() => handleDeleteEmployee(selectedEmployee.id)}>
                    Törlés
                  </button>
                </div>
              </div>

              {summary && summary.length > 0 ? (
                <div className="summary-section">
                  <h3>📊 Havi statisztika</h3>
                  <div className="summary-cards">
                    <div className="summary-card">
                      <div className="summary-value">
                        {summary.reduce((acc, day) => acc + day.hours, 0).toFixed(1)} óra
                      </div>
                      <div className="summary-label">Összes munkaidő</div>
                    </div>
                    <div className="summary-card">
                      <div className="summary-value">
                        {summary.reduce((acc, day) => acc + day.amount, 0).toLocaleString()} Ft
                      </div>
                      <div className="summary-label">Fizetés</div>
                    </div>
                    <div className="summary-card">
                      <div className="summary-value">{summary.length} nap</div>
                      <div className="summary-label">Munkában töltött napok</div>
                    </div>
                  </div>

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
                          <td>{new Date(day.day).toLocaleDateString('hu-HU')}</td>
                          <td>{day.hours.toFixed(2)} óra</td>
                          <td>{day.amount.toLocaleString()} Ft</td>
                        </tr>
                      ))}
                    </tbody>
                  </table>
                </div>
              ) : (
                <div className="no-data">Nincs adat ehhez a dolgozóhoz ebben a hónapban.</div>
              )}
            </>
          ) : (
            <div className="empty-state">
              <p>👈 Válassz egy dolgozót a listából!</p>
            </div>
          )}
        </div>
      </div>

      {showAddModal && <AddEmployeeModal onClose={() => setShowAddModal(false)} onSuccess={fetchEmployees} />}
    </div>
  );
}

// Új dolgozó hozzáadása modal
function AddEmployeeModal({ onClose, onSuccess }) {
  const [formData, setFormData] = useState({
    email: '',
    password: '',
    name: '',
    rfidUid: '',
    hourlyRate: 2000,
    role: 'Worker'
  });

  const handleSubmit = async (e) => {
    e.preventDefault();
    try {
      await authAPI.register(formData);
      alert('Dolgozó létrehozva!');
      onSuccess();
      onClose();
    } catch (error) {
      alert(error.response?.data?.message || 'Hiba a létrehozásnál!');
    }
  };

  return (
    <div className="modal-overlay" onClick={onClose}>
      <div className="modal" onClick={(e) => e.stopPropagation()}>
        <h2>Új dolgozó hozzáadása</h2>
        <form onSubmit={handleSubmit}>
          <input
            type="text"
            placeholder="Név"
            value={formData.name}
            onChange={(e) => setFormData({...formData, name: e.target.value})}
            required
          />
          <input
            type="email"
            placeholder="Email"
            value={formData.email}
            onChange={(e) => setFormData({...formData, email: e.target.value})}
            required
          />
          <input
            type="password"
            placeholder="Jelszó"
            value={formData.password}
            onChange={(e) => setFormData({...formData, password: e.target.value})}
            required
          />
          <input
            type="text"
            placeholder="RFID kártya (pl. AA:BB:CC:DD)"
            value={formData.rfidUid}
            onChange={(e) => setFormData({...formData, rfidUid: e.target.value})}
            required
          />
          <input
            type="number"
            placeholder="Órabér (Ft)"
            value={formData.hourlyRate}
            onChange={(e) => setFormData({...formData, hourlyRate: parseFloat(e.target.value)})}
            required
          />
          <select value={formData.role} onChange={(e) => setFormData({...formData, role: e.target.value})}>
            <option value="Worker">Dolgozó</option>
            <option value="Admin">Admin</option>
          </select>
          <div className="modal-actions">
            <button type="button" onClick={onClose} className="btn-secondary">Mégse</button>
            <button type="submit" className="btn-primary">Létrehozás</button>
          </div>
        </form>
      </div>
    </div>
  );
}

export default AdminDashboard;
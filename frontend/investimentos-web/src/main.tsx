import {
  StrictMode,
} from 'react'

import {
  createRoot,
} from 'react-dom/client'

import './index.css'

import {
  AuthProvider,
} from './auth/AuthContext'

import {
  AuthGate,
} from './auth/AuthGate'

createRoot(
  document.getElementById(
    'root',
  )!,
).render(
  <StrictMode>
    <AuthProvider>
      <AuthGate />
    </AuthProvider>
  </StrictMode>,
)
import { type SubmitEvent, useState } from 'react'
import { useCreateTask } from '../hooks/useCreateTask'
import type { TaskCategory } from '../types'

const CATEGORIES: TaskCategory[] = ['Courses', 'Logistique', 'Autre']

interface AddTaskFormProps {
  eventId: string
}

export function AddTaskForm({ eventId }: AddTaskFormProps) {
  const [title, setTitle] = useState('')
  const [category, setCategory] = useState<TaskCategory>('Courses')
  const [quantity, setQuantity] = useState('')
  const { mutate, isPending, error } = useCreateTask(eventId)

  function handleSubmit(event: SubmitEvent<HTMLFormElement>) {
    event.preventDefault()
    mutate(
      { title, category, quantity: quantity || undefined },
      {
        onSuccess: () => {
          setTitle('')
          setCategory('Courses')
          setQuantity('')
        },
      },
    )
  }

  return (
    <form onSubmit={handleSubmit} className="flex flex-col gap-3">
      <h2 className="text-sm font-semibold text-gray-900">Ajouter une tâche</h2>

      <div className="flex flex-col gap-1.5">
        <label htmlFor="taskTitle" className="text-sm font-medium text-gray-700">
          Titre
        </label>
        <input
          id="taskTitle"
          name="taskTitle"
          type="text"
          required
          value={title}
          onChange={(event) => setTitle(event.target.value)}
          placeholder="Bûche au chocolat"
          className="rounded-lg border border-gray-300 px-4 py-3 text-base focus:border-gray-900 focus:outline-none focus:ring-1 focus:ring-gray-900"
        />
      </div>

      <div className="flex gap-2">
        <div className="flex flex-1 flex-col gap-1.5">
          <label htmlFor="taskCategory" className="text-sm font-medium text-gray-700">
            Catégorie
          </label>
          <select
            id="taskCategory"
            name="taskCategory"
            value={category}
            onChange={(event) => setCategory(event.target.value as TaskCategory)}
            className="rounded-lg border border-gray-300 px-4 py-3 text-base text-gray-900 focus:border-gray-900 focus:outline-none focus:ring-1 focus:ring-gray-900"
          >
            {CATEGORIES.map((value) => (
              <option key={value} value={value}>
                {value}
              </option>
            ))}
          </select>
        </div>

        <div className="flex flex-1 flex-col gap-1.5">
          <label htmlFor="taskQuantity" className="text-sm font-medium text-gray-700">
            Quantité
          </label>
          <input
            id="taskQuantity"
            name="taskQuantity"
            type="text"
            value={quantity}
            onChange={(event) => setQuantity(event.target.value)}
            placeholder="1"
            className="rounded-lg border border-gray-300 px-4 py-3 text-base focus:border-gray-900 focus:outline-none focus:ring-1 focus:ring-gray-900"
          />
        </div>
      </div>

      {error && <p className="text-sm text-red-600">{error.message}</p>}

      <button
        type="submit"
        disabled={isPending}
        className="rounded-lg bg-gray-900 px-4 py-3 text-base font-medium text-white transition-colors disabled:opacity-50"
      >
        {isPending ? 'Ajout en cours…' : 'Ajouter la tâche'}
      </button>
    </form>
  )
}
